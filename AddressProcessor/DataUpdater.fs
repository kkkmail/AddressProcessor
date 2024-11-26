namespace Softellect.AddressProcessor

module DataUpdater =

    // 'P - parameter to control what to load during update
    // 'S - storage type
    type IUpdater<'P, 'S> =
        abstract member init : unit -> 'S
        abstract member update : 'P -> 'S -> 'S
        abstract member remove : 'P -> 'S -> 'S


    type Updater<'T> = MailboxProcessor<'T>


    type UpdatatableStorage<'P, 'S> =
      | UpdateContent of 'P
      | GetContent of AsyncReplyChannel<'S>
      | RemoveContent of 'P


    type AsyncUpdater<'P, 'S> (updater : IUpdater<'P, 'S>) =
        let chat = Updater.Start(fun u ->
          let rec loop s = async {
            let! m = u.Receive()

            match m with
            | UpdateContent p ->
                return! loop (updater.update p s)
            | GetContent r ->
                r.Reply s
                return! loop s
            | RemoveContent p ->
                return! loop (updater.remove p s) }

          updater.init () |> loop)

        member this.updateContent p = UpdateContent p |> chat.Post
        member this.getContent () = chat.PostAndReply GetContent
