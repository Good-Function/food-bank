module Users.Templates.UsersTable

open Layout
open Oxpecker.ViewEngine
open Users.Domain

let View (users: User list) (roles: Role list) =
    table(style="layout:fixed") {
        thead() {
            th(style="width:128px; min-width:64px;") {}
            th(style="width:50%") {"Mail"}
            th(style="min-width:140px;") {"Rola"}
            th(style="width:128px") {}
        }
        tbody() {
            for user in users do
                tr() {
                    td() {
                        img(src = $"/team/{user.Id}/photo", width = 64)
                    }
                    td(){
                        user.Mail
                    }
                    td() {
                        select(){
                            for role in roles do
                                option(selected = (role.Id = user.RoleId), title = role.Description) {
                                    role.Name
                                }
                        }
                    }
                    td() {
                        a(href="") {
                            Icons.Delete
                        }
                    }
                }
        }
    }