module Applications.CompositionRoot

type Dependencies = {
    TestRead: Async<string list>
}

let build dbConnect = {
    TestRead = Database.readSchemas dbConnect
}
