module Users.CompositionRoot

type Dependencies = {
    TestRead: Async<string list>
}

let build = {
    TestRead = async {return []}
}
