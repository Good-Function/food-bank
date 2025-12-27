module Users.Templates.UsersTable

open Layout
open Oxpecker.ViewEngine
open Permissions
open Users.Domain
open Oxpecker.Htmx

let View (users: User list) (roles: Role list) permissions (antiforgeryToken: HtmlElement) =
    table() {
        antiforgeryToken
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
                        img(src = $"/team/{user.Id}/photo", width = 64, style="border-radius:50%")
                    }
                    td(){
                        user.Mail
                    }
                    td() {
                        select(
                            hxPut = $"/team/users/{user.Id}/roles",
                            hxTrigger="change",
                            hxTarget="closest table",
                            disabled = not (permissions |> List.contains Permission.ManageUsers),
                            hxSwap="outerHTML",
                            name="RoleId",
                            hxIndicator= "#UsersIndicator",
                            hxInclude = "[name='__RequestVerificationToken']"
                            ){
                            for role in roles do
                                option(selected = (role.Id = user.RoleId), title = role.Description, value=role.Id.ToString()) {
                                    role.Name
                                }
                        }
                    }
                    td() {
                        if permissions |> List.contains Permission.ManageUsers then
                            a(
                                hxTarget = "closest tr",
                                hxDelete =  $"/team/users/{user.Id}",
                                hxConfirm="Czy na pewno chcesz usunąć tego użytkownika?",
                                hxIndicator= "#UsersIndicator",
                                hxInclude = "[name='__RequestVerificationToken']",
                                href="") {
                                Icons.Delete
                            }
                        else Fragment()
                    }
                }
        }
    }