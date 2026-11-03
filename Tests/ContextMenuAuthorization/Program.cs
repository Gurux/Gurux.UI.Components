using System.Reflection;
using System.Security.Claims;
using Gurux.UI.Components;
using Gurux.UI.Components.ContextMenu;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

var provider = new Auth();
var refresh = new Refresh();
using var services = new ServiceCollection().AddLogging().AddAuthorizationCore(options=>options.AddPolicy("write",p=>p.RequireClaim("write")))
 .AddSingleton<AuthenticationStateProvider>(provider).AddSingleton<IGXAuthorizationRefreshSource>(refresh).AddSingleton<IJSRuntime>(new Js()).BuildServiceProvider();
await using var renderer = new TestRenderer(services);
await renderer.Dispatcher.InvokeAsync(async()=>{
 var calls=0;
 GXContextMenuItem? item=null;
 RenderFragment Content(string? policy)=>b=>{b.OpenComponent<GXContextMenuItem>(0);b.AddAttribute(1,"Text","Protected command");b.AddAttribute(2,"Policy",policy);b.AddAttribute(3,"OnClick",EventCallback.Factory.Create(new object(),()=>calls++));b.AddComponentReferenceCapture(4,value=>item=(GXContextMenuItem)value);b.CloseComponent();};
 var (menu,id)=await renderer.Mount<GXContextMenuController>(new(){["ChildContent"]=Content("write")});
 typeof(GXContextMenuController).GetMethod("Show",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(menu,null);
 await Task.Yield();
 var failures=new List<string>();
 void Check(bool condition,string message){if(!condition)failures.Add(message);}
 Check(!renderer.HasText(id,"Protected command"),"Denied named-policy command must not render");
 item!.Click();await Task.Yield();
 Check(calls==0,"Legacy public Click must not invoke a denied command");
 if(failures.Count!=0)throw new Exception(string.Join("; ",failures));
 provider.Set(true);await Task.Yield();await Task.Yield();
 typeof(GXContextMenuController).GetMethod("Show",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(menu,null);await Task.Yield();
 Check(renderer.HasText(id,"Protected command"),"Grant must refresh rendered context commands");
 await Invoke(item!);Check(calls==1,"Allowed command must invoke exactly once");
 provider.Set(false,false);
 await Invoke(item!);Check(calls==1,"Invocation must recheck current policy without a refresh notification");
 refresh.Invalidate();await Task.Yield();await Task.Yield();
 typeof(GXContextMenuController).GetMethod("Show",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(menu,null);await Task.Yield();
 Check(!renderer.HasText(id,"Protected command"),"Refresh source must hide revoked commands");
 await renderer.Update(id,new(){["ChildContent"]=Content(null)});
 typeof(GXContextMenuController).GetMethod("Show",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(menu,null);await Task.Yield();
 Check(renderer.HasText(id,"Protected command"),"Blank policies must remain unprotected");
 var removed=item!;await renderer.Update(id,new(){["ChildContent"]=(RenderFragment?)null});await Task.Yield();
 Check(!renderer.HasText(id,"Protected command"),"Removed declarative items must unregister");
 await Invoke(removed);Check(calls==1,"Removed item references must not execute");
 if(failures.Count!=0)throw new Exception(string.Join("; ",failures));
 Console.WriteLine("Context policy visibility, guarded legacy/direct invocation, auth and permission refresh, dynamic policy and item removal passed.");
});
using var noAuthServices=new ServiceCollection().AddLogging().AddSingleton<IJSRuntime>(new Js()).BuildServiceProvider();
await using var noAuthRenderer=new TestRenderer(noAuthServices);
await noAuthRenderer.Dispatcher.InvokeAsync(async()=>{
 var calls=0;
 var items=new List<GXContextMenuItem>();
 RenderFragment content=b=>{
  foreach(var policy in new string?[]{"write",null}){
   b.OpenComponent<GXContextMenuItem>(0);b.AddAttribute(1,"Text",policy==null?"Public command":"Private command");b.AddAttribute(2,"Policy",policy);b.AddAttribute(3,"OnClick",EventCallback.Factory.Create(new object(),()=>calls++));b.AddComponentReferenceCapture(4,value=>items.Add((GXContextMenuItem)value));b.CloseComponent();
  }
 };
 var (menu,id)=await noAuthRenderer.Mount<GXContextMenuController>(new(){["ChildContent"]=content});
 typeof(GXContextMenuController).GetMethod("Show",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(menu,null);await Task.Yield();
 if(noAuthRenderer.HasText(id,"Private command") || !noAuthRenderer.HasText(id,"Public command"))throw new Exception("Missing authorization services must deny named policies and preserve public commands");
 await Invoke(items.First(item=>item.Policy=="write"));
 await Invoke(items.First(item=>item.Policy==null));
 if(calls!=1)throw new Exception("Missing auth services must permit only the public callback");
 Console.WriteLine("Missing authorization services fail closed for context policies and preserve unprotected commands.");
});
static Task Invoke(GXContextMenuItem item) => (Task)(item.GetType().GetMethod("ClickAsync")?.Invoke(item,null) ?? throw new Exception("Guarded ClickAsync is missing"));
sealed class Auth:AuthenticationStateProvider {
 private bool allowed;
 public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(allowed?[new Claim("write","true")]:[],"test"))));
 public void Set(bool value,bool notify=true){allowed=value;if(notify)NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
}
sealed class Refresh:IGXAuthorizationRefreshSource{public event Action? Changed;public void Invalidate()=>Changed?.Invoke();}
sealed class Js:IJSRuntime,IJSObjectReference {
 public ValueTask<TValue> InvokeAsync<TValue>(string identifier,object?[]? args)=>ValueTask.FromResult(identifier=="import"?(TValue)(object)this:default!);
 public ValueTask<TValue> InvokeAsync<TValue>(string identifier,CancellationToken token,object?[]? args)=>InvokeAsync<TValue>(identifier,args);
 public ValueTask DisposeAsync()=>ValueTask.CompletedTask;
}
sealed class TestRenderer(IServiceProvider services):Renderer(services,services.GetRequiredService<ILoggerFactory>()) {
 public override Dispatcher Dispatcher{get;}=Dispatcher.CreateDefault();
 public async Task<(T,int)> Mount<T>(Dictionary<string,object?> parameters) where T:IComponent {var c=(T)InstantiateComponent(typeof(T));var id=AssignRootComponentId(c);await Update(id,parameters);return(c,id);}
 public Task Update(int id,Dictionary<string,object?> parameters)=>RenderRootComponentAsync(id,ParameterView.FromDictionary(parameters));
 public bool HasText(int id,string value){var frames=GetCurrentRenderTreeFrames(id);return frames.Array.Take(frames.Count).Any(f=>f.FrameType==RenderTreeFrameType.Text&&f.TextContent==value || f.FrameType==RenderTreeFrameType.Component&&HasText(f.ComponentId,value));}
 protected override void HandleException(Exception error)=>throw error;
 protected override Task UpdateDisplayAsync(in RenderBatch batch)=>Task.CompletedTask;
}
