module E2EPlaywright

open KestrelTestServer
open Microsoft.Playwright
open Xunit

let screenshot name (page: IPage) =
    task {
        let screenshotOptions = PageScreenshotOptions(Path=($"{name}.png"))
        let! _ = page.ScreenshotAsync(screenshotOptions)
        ()
    }

[<Fact>]
let ``User can log in and see organizations page``() =
    task {
        // Arrange
        use app = new KestrelApplicationFactory()
        use! playwright = Playwright.CreateAsync()
        use! browser = playwright.Chromium.LaunchAsync()
        let! page = browser.NewPageAsync()
        let! _ = page.GotoAsync (app.ServerAddress + "login")
        let! emailInput = page.QuerySelectorAsync "input[name='Email']"
        let! passwordInput = page.QuerySelectorAsync "input[name='Password']"
        let! submitButton = page.QuerySelectorAsync "input[type='submit'][name='Login']"
        // Act      
        let! _ = emailInput.FillAsync "admin@admin.pl"
        let! _ = passwordInput.FillAsync "f00d!"
        let! _ = submitButton.ClickAsync()
        // Assert
        do! Assertions.Expect(page.Locator("table")).ToBeAttachedAsync()
}