# food-bank-operator

## 1. Getting started
1. Clone the repo, ensure you have docker and .net 9 installed.
2. Set env variable: `echo 'export ASPNETCORE_ENVIRONMENT=Development' >> ~/.bashrc`
3. cd to OperatorPortal 
4. `dotnet restore`
5. install playwright `pwsh Tests/bin/Debug/net9.0/playwright.ps1 install chromium --with-deps`. Yes you need powershell (it can be installed on mac and ubuntu). See official docs https://playwright.dev/dotnet/docs/intro#introduction.
6. `dotnet test` 
7. dotnet run --project Web

## 2. Backlog
Can be found here: 
https://github.com/orgs/Good-Function/projects/2/views/1
On the top you can find most important tasks.
