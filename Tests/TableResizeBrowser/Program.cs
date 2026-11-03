using Microsoft.Playwright;

var source = Path.GetFullPath(args.ElementAtOrDefault(0) ?? Path.Combine(AppContext.BaseDirectory, "../../../../../Development/wwwroot/Table/widths.js"));
var module = "data:text/javascript;base64," + Convert.ToBase64String(await File.ReadAllBytesAsync(source));
using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
var page = await browser.NewPageAsync();
await page.SetContentAsync("<table id='old'><thead><tr><th data-gx-column='Name'>Name</th></tr></thead></table>");
// Reproduce a module import finishing after navigation removes the old table.
await page.EvaluateAsync("""
    async module => {
        const widths = await import(module);
        document.getElementById('old').remove();
        const component = { invokeMethodAsync() { throw new Error('Disposed component was invoked'); } };
        const late = widths.initialize('old', component);
        late.dispose();
        late.dispose();
        widths.initialize('never-rendered', component).dispose();
    }
    """, module);
Console.WriteLine("PASS: late initialization after navigation and missing table are safe and disposable.");
await page.SetContentAsync("""
    <style>th { box-sizing:border-box; width:120px; min-width:120px; max-width:120px; padding:0; border:0; }</style>
    <table id='healthy'><thead><tr><th data-gx-column='Name'>Name</th></tr></thead></table>
    """);
await page.EvaluateAsync("""
    async module => {
        window.saves = [];
        const widths = await import(module);
        window.handler = widths.initialize('healthy', { invokeMethodAsync(...args) { window.saves.push(args); return Promise.resolve(); } });
    }
    """, module);
var header = page.Locator("th");
var box = (await header.BoundingBoxAsync())!;
await page.Mouse.MoveAsync(box.X + box.Width - 2, box.Y + box.Height / 2);
await page.Mouse.DownAsync();
await page.Mouse.MoveAsync(box.X + box.Width + 38, box.Y + box.Height / 2);
await page.Mouse.UpAsync();
if (!await page.EvaluateAsync<bool>("window.saves.length === 1 && window.saves[0][0] === 'SaveColumnWidthAsync' && window.saves[0][1] === 'Name' && Math.abs(window.saves[0][2] - 160) < 1"))
    throw new Exception("Healthy resize must save the changed column width.");
if (!await header.EvaluateAsync<bool>("e => e.style.width === '160px' && e.style.minWidth === '160px' && e.style.maxWidth === '160px'"))
    throw new Exception("Healthy resize must apply all three width constraints.");
await page.EvaluateAsync("window.handler.dispose(); window.handler.dispose()");
box = (await header.BoundingBoxAsync())!;
await page.Mouse.MoveAsync(box.X + box.Width - 2, box.Y + box.Height / 2);
await page.Mouse.DownAsync();
await page.Mouse.MoveAsync(box.X + box.Width + 38, box.Y + box.Height / 2);
await page.Mouse.UpAsync();
if (!await page.EvaluateAsync<bool>("window.saves.length === 1")) throw new Exception("Disposed listeners must not save widths.");
Console.WriteLine("PASS: healthy drag applies and persists width; disposing removes listeners and is idempotent.");
