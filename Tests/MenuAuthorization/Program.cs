using System.Reflection;
using System.Security.Claims;
using Gurux.UI.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

var state = new MenuAuthentication();
var refresh = new MenuRefresh();
using var menus = new GXMenuService();
using var services = new ServiceCollection().AddLogging()
    .AddAuthorizationCore(options =>
    {
        foreach (var policy in new[] { "script.add", "script.edit", "parent.open", "child.run" })
            options.AddPolicy(policy, rule => rule.RequireAuthenticatedUser().RequireClaim("Permission", policy));
    })
    .AddSingleton<AuthenticationStateProvider>(state).AddSingleton<IGXAuthorizationRefreshSource>(refresh)
    .AddSingleton<IGXMenuService>(menus).AddSingleton<IGXProgress>(new GXProgressService())
    .AddSingleton<IJSRuntime>(new FocusRuntime()).AddSingleton<NavigationManager>(new MenuNavigation()).BuildServiceProvider();
await using var renderer = new AuthorizationRenderer(services);
await renderer.Dispatcher.InvokeAsync(async () =>
{
    var clicks = 0;
    var protectedItem = Command("Protected", "script.add", () => clicks++);
    var free = Command("Free", null, () => clicks++);
    var (_, id) = await renderer.MountAsync<GXMenuControl>(new() { ["MenuItems"] = new List<GXMenuEntry> { free, protectedItem } });
    Check(renderer.HasLabel(id, "Free"), "Unprotected commands must remain visible");
    Check(!renderer.HasLabel(id, "Protected"), "Denied named-policy command must not render");
    var root = (GXMenuControl)renderer.Root(id);
    await Select(root, protectedItem);
    Check(clicks == 0, "Direct handler invocation must not execute a denied command");
    state.Set(["script.add"]);
    await renderer.Dispatcher.InvokeAsync(() => Task.CompletedTask);
    Check(renderer.HasLabel(id, "Protected"), "Grant must make the authorized command visible");
    await renderer.ClickCommandAsync(id, "Protected");
    Check(clicks == 1, "Allowed command must execute exactly once");
    state.Set([], notify: false);
    await Select(root, protectedItem);
    Check(clicks == 1, "Click guard must recheck current state even before a notification rerenders the menu");
    state.Set([]);
    await renderer.Dispatcher.InvokeAsync(() => Task.CompletedTask);
    Check(!renderer.HasLabel(id, "Protected"), "Revocation must hide the old command");
    protectedItem.Policy = " ";
    await renderer.UpdateAsync(id, new() { ["MenuItems"] = new List<GXMenuEntry> { protectedItem } });
    Check(renderer.HasLabel(id, "Protected"), "Blank policies must retain unprotected behavior");
    protectedItem.Policy = "unknown-policy";
    await renderer.UpdateAsync(id, new() { ["MenuItems"] = new List<GXMenuEntry> { protectedItem } });
    Check(!renderer.HasLabel(id, "Protected"), "Unknown policies must fail closed");
    protectedItem.Policy = "script.add";
    state.Set(["script.add"]);
    await renderer.UpdateAsync(id, new() { ["MenuItems"] = new List<GXMenuEntry> { protectedItem }, ["ReadOnly"] = true });
    await Select(root, protectedItem);
    Check(clicks == 1, "ReadOnly must still prevent allowed command execution");
    await renderer.UpdateAsync(id, new() { ["ReadOnly"] = false, ["MenuItems"] = new List<GXMenuEntry>() });
    await Select(root, protectedItem);
    Check(clicks == 1, "Removed commands must not execute through an old handler");

    var changingPolicyDenied = false;
    var calledWithoutPermission = false;
    var changing = Command("Changing", "script.add", () => calledWithoutPermission = changingPolicyDenied);
    changingPolicyDenied = false;
    var (_, changingId) = await renderer.MountAsync<GXMenuControl>(new()
    {
        ["MenuItems"] = new List<GXMenuEntry> { changing },
        ["OnSelected"] = EventCallback.Factory.Create(new object(), () =>
        {
            changingPolicyDenied = true;
            changing.Policy = "script.edit";
        })
    });
    await renderer.ClickCommandAsync(changingId, "Changing");
    Check(!calledWithoutPermission, "Selection notification must not create an authorization gap before command invocation");

    string childPolicy = "script.edit";
    RenderFragment declared = builder =>
    {
        builder.OpenComponent<GXMenuEntry>(0);
        builder.AddAttribute(1, "Text", "Declared");
        builder.AddAttribute(2, "Policy", childPolicy);
        builder.AddAttribute(3, "OnClick", EventCallback.Factory.Create(new object(), () => clicks++));
        builder.CloseComponent();
    };
    var (_, declaredId) = await renderer.MountAsync<GXMenuControl>(new() { ["ChildContent"] = declared });
    Check(!renderer.HasLabel(declaredId, "Declared"), "Declarative child policy must hide denied commands");
    childPolicy = "script.add";
    await renderer.UpdateAsync(declaredId, new() { ["ChildContent"] = declared });
    Check(renderer.HasLabel(declaredId, "Declared"), "Changing a declarative item's policy must reevaluate the parent");
    await renderer.UpdateAsync(declaredId, new() { ["ChildContent"] = null });
    Check(!renderer.HasLabel(declaredId, "Declared"), "Disposed declarations must unregister");

    var child = Command("Child", "child.run", () => clicks++);
    var parent = Command("Parent", "parent.open", () => throw new Exception("Dropdown parent must not execute"));
    parent.Children = [child];
    var (_, treeId) = await renderer.MountAsync<GXMenuControl>(new() { ["MenuItems"] = new List<GXMenuEntry> { parent } });
    Check(!renderer.HasLabel(treeId, "Parent"), "Denied parent must hide its subtree");
    state.Set(["parent.open"]);
    Check(!renderer.HasLabel(treeId, "Parent"), "A branch with only denied children must not offer an empty command");
    state.Set(["parent.open", "child.run"]);
    Check(renderer.HasLabel(treeId, "Parent"), "Authorized branch must remain available");
    var nestedId = renderer.FindControl(treeId, "Parent");
    var nested = (GXMenuControl)renderer.Nested(treeId, nestedId);
    await renderer.ClickToggleAsync(nestedId);
    Check(renderer.HasLabel(nestedId, "Child"), "Authorized child must render after opening the dropdown");
    state.Set(["child.run"], notify: false);
    await Select(nested, child);
    Check(clicks == 1, "Child click must recheck the parent policy");
    state.Set(["parent.open", "child.run"]);
    await renderer.ClickToggleAsync(nestedId);
    await Select(nested, child);
    Check(clicks == 2, "An authorized nested command must execute once");

    state.Set([], notify: false);
    refresh.Invalidate();
    Check(!renderer.HasLabel(treeId, "Parent"), "Application authorization refresh must hide revoked policy decisions");

    state.Set(["parent.open", "child.run"]);
    nestedId = renderer.FindControl(treeId, "Parent");
    nested = (GXMenuControl)renderer.Nested(treeId, nestedId);
    state.BlockAfter(1); // Parent check succeeds, then the child authentication read waits.
    var pendingSelection = Select(nested, child);
    await state.Blocked.WaitAsync(TimeSpan.FromSeconds(5));
    parent.ReadOnly = true;
    state.Release();
    await pendingSelection;
    Check(clicks == 2, "Parent access must be rechecked after an asynchronous child authorization");
    parent.ReadOnly = false;
    state.BlockAfter(1);
    var pendingToggle = renderer.ClickToggleAsync(nestedId);
    await state.Blocked.WaitAsync(TimeSpan.FromSeconds(5));
    parent.ReadOnly = true;
    state.Release();
    await pendingToggle;
    Check(!renderer.IsExpanded(nestedId), "A submenu must not open when its parent becomes inaccessible during authorization refresh");
    parent.ReadOnly = false;
    state.SetAnonymous();
    var (_, anonymousId) = await renderer.MountAsync<GXMenuControl>(new() { ["MenuItems"] = new List<GXMenuEntry> { free, protectedItem } });
    Check(renderer.HasLabel(anonymousId, "Free") && !renderer.HasLabel(anonymousId, "Protected"), "Anonymous users must see only unprotected commands");
});
await renderer.Dispatcher.InvokeAsync(async () =>
{
    state.Set(["script.add"]);
    var clicks = 0;
    GXMenuItem TopCommand(string text, string? policy) 
    {
        var item = new GXMenuItem(text, null, EventCallback.Factory.Create(new object(), () => clicks++));
        typeof(GXMenuItem).GetProperty("Policy")?.SetValue(item, policy);
        return item;
    }
    var allowed = TopCommand("Top Add", "script.add");
    var denied = TopCommand("Top Edit", "script.edit");
    var parent = TopCommand("Top Parent", "script.edit");
    parent.Children = [TopCommand("Top Child", "script.add")];
    using var registration = menus.RegisterMenu([allowed, denied, parent]);
    var (bar, barId) = await renderer.MountAsync<GXMenuBar>(new());
    Check(renderer.HasLabel(barId, "Top Add") && !renderer.HasLabel(barId, "Top Edit"), "Top bar must enforce command policy visibility");
    Check(!renderer.HasLabel(barId, "Top Parent"), "Top bar must enforce dropdown parent policies");
    var invoke = typeof(GXMenuBar).GetMethod("InvokeCommandAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
    await (Task)invoke.Invoke(bar, [denied])!;
    Check(clicks == 0, "Top-bar direct handler must deny a forbidden command");
    await renderer.ClickCommandAsync(barId, "Top Add");
    Check(clicks == 1, "Top-bar allowed command control must succeed");
    state.Set([], notify: false);
    await (Task)invoke.Invoke(bar, [allowed])!;
    Check(clicks == 1, "Top bar must check current policy before invoking a cached command");
    state.Set([]);
    Check(!renderer.HasLabel(barId, "Top Add"), "Top bar must hide revoked commands");
    state.Set(["script.add", "script.edit"]);
    Check(renderer.HasLabel(barId, "Top Parent"), "Granted top-bar parent must become available");
    var dropdown = renderer.FindControl(barId, "Top Parent");
    await renderer.ClickToggleAsync(dropdown);
    Check(renderer.HasLabel(dropdown, "Top Child"), "Authorized top-bar dropdown child must render");
    state.Set(["script.add"], notify: false);
    await (Task)invoke.Invoke(bar, [parent.Children.Single()])!;
    Check(clicks == 1, "Top-bar child must not bypass a revoked ancestor policy");
    registration.Dispose();
    await (Task)invoke.Invoke(bar, [allowed])!;
    Check(clicks == 1, "Removed top-bar registration must not execute an old callback");
});
using var noAuthorization = new ServiceCollection().AddLogging().AddSingleton<IJSRuntime>(new FocusRuntime())
    .AddSingleton<IGXProgress>(new GXProgressService()).AddSingleton<NavigationManager>(new MenuNavigation()).BuildServiceProvider();
await using var plainRenderer = new AuthorizationRenderer(noAuthorization);
await plainRenderer.Dispatcher.InvokeAsync(async () =>
{
    var (_, id) = await plainRenderer.MountAsync<GXMenuControl>(new()
    {
        ["MenuItems"] = new List<GXMenuEntry> { Command("Public", null, () => { }), Command("Protected", "script.add", () => throw new Exception("No auth services")) }
    });
    Check(plainRenderer.HasLabel(id, "Public") && !plainRenderer.HasLabel(id, "Protected"), "Missing authorization services must allow only unprotected commands");
});
var delayedState = new MenuAuthentication();
delayedState.Set(["script.add"], notify: false);
delayedState.BlockAfter(0);
using var delayedServices = new ServiceCollection().AddLogging()
    .AddAuthorizationCore(options => options.AddPolicy("script.add", rule => rule.RequireAuthenticatedUser().RequireClaim("Permission", "script.add")))
    .AddSingleton<AuthenticationStateProvider>(delayedState).AddSingleton<IJSRuntime>(new FocusRuntime())
    .AddSingleton<IGXProgress>(new GXProgressService()).AddSingleton<NavigationManager>(new MenuNavigation()).BuildServiceProvider();
await using var delayedRenderer = new AuthorizationRenderer(delayedServices);
var pendingMount = delayedRenderer.Dispatcher.InvokeAsync(() => delayedRenderer.MountAsync<GXMenuControl>(new()
{
    ["MenuItems"] = new List<GXMenuEntry> { Command("Delayed", "script.add", () => { }) }
}));
await delayedState.Blocked.WaitAsync(TimeSpan.FromSeconds(5));
await delayedRenderer.Dispatcher.InvokeAsync(() => delayedState.Set([]));
delayedState.Release();
var (_, delayedId) = await pendingMount;
Check(!delayedRenderer.HasLabel(delayedId, "Delayed"), "A stale asynchronous authorization result must not restore a revoked command");
Console.WriteLine("Menu policies: data/declarative visibility, allowed/denied callbacks, grant/revocation, dynamic policies, unknown/missing services, ReadOnly, disposal, nested parent checks and policy refresh passed.");
static GXMenuEntry Command(string text, string? policy, Action callback) => new()
{
    Text = text, Policy = policy, OnClick = EventCallback.Factory.Create(new object(), callback)
};
static async Task Select(GXMenuControl control, GXMenuEntry item) =>
    await (Task)typeof(GXMenuControl).GetMethod("SelectAsync", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(control, [item])!;
static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
sealed class MenuAuthentication : AuthenticationStateProvider
{
    private AuthenticationState state = new(new ClaimsPrincipal(new ClaimsIdentity([], "test")));
    private TaskCompletionSource<AuthenticationState>? gate;
    private AuthenticationState? blockedState;
    private TaskCompletionSource observed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int readsBeforeBlock = -1;
    public Task Blocked => observed.Task;
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (readsBeforeBlock >= 0 && readsBeforeBlock-- == 0)
        {
            blockedState = state;
            observed.TrySetResult();
            return gate!.Task;
        }
        return Task.FromResult(state);
    }
    public void BlockAfter(int reads)
    {
        gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        observed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        readsBeforeBlock = reads;
    }
    public void Release() => gate!.TrySetResult(blockedState ?? state);
    public void SetAnonymous()
    {
        state = new(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }
    public void Set(string[] permissions, bool notify = true)
    {
        state = new(new ClaimsPrincipal(new ClaimsIdentity(permissions.Select(p => new Claim("Permission", p)), "test")));
        if (notify) NotifyAuthenticationStateChanged(Task.FromResult(state));
    }
}
sealed class MenuRefresh : IGXAuthorizationRefreshSource
{
    public event Action? Changed;
    public void Invalidate() => Changed?.Invoke();
}

sealed class AuthorizationRenderer : Renderer
{
    public AuthorizationRenderer(IServiceProvider services) : base(services, services.GetRequiredService<ILoggerFactory>())
        => ElementReferenceContext = new WebElementReferenceContext(services.GetRequiredService<IJSRuntime>());
    public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
    public async Task<(T Component, int Id)> MountAsync<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        var component = (T)InstantiateComponent(typeof(T));
        var id = AssignRootComponentId(component);
        await UpdateAsync(id, parameters);
        return (component, id);
    }
    public object Root(int id) => GetComponentState(id).Component;
    public object Nested(int root, int nested) => Frames(root).Single(f => f.FrameType == RenderTreeFrameType.Component && f.ComponentId == nested).Component;
    public Task UpdateAsync(int id, Dictionary<string, object?> parameters)
        => RenderRootComponentAsync(id, ParameterView.FromDictionary(parameters));
    public bool HasAttribute(int id, string name, string? value = null)
        => Frames(id).Any(frame => frame.FrameType == RenderTreeFrameType.Attribute && frame.AttributeName == name
            && (value == null || frame.AttributeValue?.ToString() == value));
    public bool IsExpanded(int id)
    {
        var attribute = Frames(id).FirstOrDefault(frame => frame.FrameType == RenderTreeFrameType.Attribute && frame.AttributeName == "aria-expanded");
        if (attribute.AttributeName == null) throw new Exception("Dropdown must render a toggle with aria-expanded.");
        return string.Equals(attribute.AttributeValue?.ToString(), "true", StringComparison.OrdinalIgnoreCase);
    }
    public Task ClickToggleAsync(int id) => ClickAsync(id, "aria-expanded");
    public bool StopsKeyPropagation(int id) => Frames(id).Any(frame => frame.FrameType == RenderTreeFrameType.Attribute
        && frame.AttributeName == "__internal_stopPropagation_onkeydown" && Equals(frame.AttributeValue, true));
    public bool HasSeparator(int id) => HasAttribute(id, "role", "separator")
        || Frames(id).Any(frame => frame.FrameType == RenderTreeFrameType.Markup && frame.MarkupContent.Contains("role=\"separator\""));
    public int FindControl(int id, string text)
        => Frames(id).Single(frame => frame.FrameType == RenderTreeFrameType.Component
            && frame.Component is GXMenuControl control && control.Text == text).ComponentId;
    public string ToggleReference(int id)
        => Frames(id).Single(frame => frame.FrameType == RenderTreeFrameType.ElementReferenceCapture).ElementReferenceCaptureId;
    public bool HasLabel(int id, string text)
        => Frames(id).Any(frame => frame.FrameType == RenderTreeFrameType.Component && frame.Component is GXLocalizedText label && label.Value == text);
    public bool CommandDisabled(int id, string text)
        => CommandAttributes(id, text).Any(frame => frame.AttributeName == "disabled" && Equals(frame.AttributeValue, true));
    public Task ClickCommandAsync(int id, string text)
        => DispatchEventAsync(CommandAttributes(id, text).Single(frame => frame.AttributeName == "onclick").AttributeEventHandlerId,
            null, new MouseEventArgs());
    private RenderTreeFrame[] CommandAttributes(int id, string text)
    {
        var frames = Frames(id);
        for (var index = 0; index < frames.Length; ++index)
        {
            if (frames[index].FrameType != RenderTreeFrameType.Element || frames[index].ElementName != "button") continue;
            var element = frames.Skip(index + 1).Take(frames[index].ElementSubtreeLength - 1).ToArray();
            if (element.Any(frame => frame.FrameType == RenderTreeFrameType.Component && frame.Component is GXLocalizedText label && label.Value == text))
                return element.TakeWhile(frame => frame.FrameType == RenderTreeFrameType.Attribute).ToArray();
        }
        throw new Exception($"No button labelled {text}.");
    }
    public Task ClickAsync(int id, string attribute, string? value = null)
    {
        var frames = Frames(id);
        for (var index = 0; index < frames.Length; ++index)
        {
            if (frames[index].FrameType != RenderTreeFrameType.Element) continue;
            var attributes = frames.Skip(index + 1).TakeWhile(frame => frame.FrameType == RenderTreeFrameType.Attribute).ToArray();
            if (attributes.Any(frame => frame.AttributeName == attribute && (value == null || frame.AttributeValue?.ToString() == value)))
            {
                var handler = attributes.Single(frame => frame.AttributeName == "onclick").AttributeEventHandlerId;
                return DispatchEventAsync(handler, null, new MouseEventArgs());
            }
        }
        throw new Exception($"No clickable element with {attribute}={value}.");
    }
    public Task KeyAsync(int id, string key)
        => DispatchEventAsync(Frames(id).Single(frame => frame.FrameType == RenderTreeFrameType.Attribute
            && frame.AttributeName == "onkeydown").AttributeEventHandlerId, null, new KeyboardEventArgs { Key = key });
    private RenderTreeFrame[] Frames(int id)
    {
        var frames = GetCurrentRenderTreeFrames(id);
        var result = frames.Array.Take(frames.Count).ToList();
        foreach (var frame in result.ToArray())
        {
            if (frame.FrameType == RenderTreeFrameType.Component)
            {
                result.AddRange(Frames(frame.ComponentId));
            }
        }
        return result.ToArray();
    }
    protected override void HandleException(Exception exception) => throw exception;
    protected override Task UpdateDisplayAsync(in RenderBatch batch) => Task.CompletedTask;
}

sealed class MenuNavigation : NavigationManager
{
    public MenuNavigation() => Initialize("http://test/", "http://test/");
    protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException();
    protected override void SetNavigationLockState(bool value) { }
}

sealed class FocusRuntime : IJSRuntime
{
    public string? LastTarget { get; private set; }
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        if (args?.FirstOrDefault() is ElementReference element)
        {
            LastTarget = element.Id;
        }
        return ValueTask.FromResult(default(TValue)!);
    }
}
