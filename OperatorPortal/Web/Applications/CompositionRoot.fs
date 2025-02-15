module Applications.CompositionRoot

type Dependencies = {
    TestRead: Async<string list>
}