# food-bank-operator

## 1. Getting started
1. Clone the repo, ensure you have docker and .net 9 installed.
2. Set env variable: `echo 'export ASPNETCORE_ENVIRONMENT=Development' >> ~/.bashrc`
3. cd to OperatorPortal 
4. `dotnet restore`
5. install playwright `pwsh Tests/bin/Debug/net9.0/playwright.ps1 install chromium --with-deps`. Yes you need powershell (it can be installed on mac and ubuntu). See official docs https://playwright.dev/dotnet/docs/intro#introduction.
6. `dotnet test` 
7. dotnet run --project Web

## Extras
TestContainers manages local dependencies. It may be useful to install https://azure.microsoft.com/en-us/products/storage/storage-explorer#Download-4 to view data from emulator: Azurite.

## 2. Backlog
Can be found here: 
https://github.com/orgs/Good-Function/projects/2/views/1
On the top you can find most important tasks.

## CharityPortal
1. Create & fill .env file based on .env.example
2. Execute `source .env && air` 


## Useful stuff
1. Lint bicep: 
`az bicep lint --file ./infra.bicep`
2. Validate bicep (requires login):
`az deployment group validate --resource-group bzsoswaw --template-file ./infra.bicep`
