namespace Softellect.AddressProcessor

open CSharpInterop
open StringParser
open AddressTypes

module MatchTypes =

    /// Partial match - the SMALLER is p the BETTER is the match.
    type MatchResult =
        | Perfect // Perfect match - any comparison must put earlier rule with perfect match at the top ==> no need to consider any further rules of that type.
        | Partial of float // Partial match. Address list and unprocessed in MathOutput are updated. The caller will decide what to do.
        | Failed // If failed then Address list and unprocessed (below) in MatchOutput are not updated.

        static member almostFailing = Partial 1000.0

        /// Numerical projection of MatchResult.
        member this.value =
            match this with
            | Perfect -> 0.0
            | Partial p -> max 0.0 p
            | Failed -> 1.0e9

        member this.isPassing : bool =
            match this with
            | Perfect -> true
            | Failed -> false
            | Partial p ->
                if p < 0.6 then true
                else false

        static member (+) (m1 : MatchResult, m2 : MatchResult) : MatchResult =
            match m1, m2 with
            | Perfect, _ -> m2
            | _, Perfect -> m1
            | Failed, _ -> Failed
            | _, Failed -> Failed
            | Partial p1, Partial p2 -> Partial (p1 + p2)

        static member (+) (m1 : MatchResult, p2 : float) : MatchResult = m1 + (Partial p2)
        static member (+) (p1 : float, m2 : MatchResult) : MatchResult = (Partial p1) + m2

        static member (/) (m : MatchResult, d : int) : MatchResult =
            match m with
            | Perfect -> Perfect
            | Failed -> Failed
            | Partial p1 -> Partial (p1 / (float d))

        static member (/) (m : MatchResult, d : float) : MatchResult =
            match m with
            | Perfect -> Perfect
            | Failed -> Failed
            | Partial p1 -> Partial (p1 / d)

        static member inline DivideByInt (m : MatchResult, d : int) = m / d
        static member Zero = Perfect


    /// Calculates average of all not failed results from given list<MatchResult>.
    /// If all failed, then returns a given default value.
    let averageOrDefault (d : MatchResult) v =
        let w = v |> List.filter (fun e -> e = Failed |> not)

        match w with
        | [] -> d
        | _ -> w |> List.average


    type StepResult =
        | Matched
        | Inferred
        | NotMatched


    type DetailedStepResult =
        {
            numberResult :StepResult
            streetResult : StepResult
            unitResult : StepResult
            cityResult : StepResult
            stateResult : StepResult
            zipResult : StepResult
        }

        static member defaultValue =
            {
                numberResult = NotMatched
                streetResult = NotMatched
                unitResult = NotMatched
                cityResult = NotMatched
                stateResult = NotMatched
                zipResult = NotMatched
            }


    type MatchError =
        | ZipNotFound
        | StateNotFound
        | CityNotFound
        | StreetNotFound
        | InvalidHouseNumber
        | InvalidUnitNumber
        | InvalidAddress
        | TooLarge
        | DuplicateAddress
        | HouseNumberNotFound
        | CriticalError


    type ResolvedResult =
        {
            resolvedAddress : Address
            resolvedResult : MatchResult
            resolvedStepResults : DetailedStepResult
        }


    type MatchInfo =
        {
            result : MatchResult
            stepResults : DetailedStepResult
            resolved : list<ResolvedResult> // Currently recognized address(-es) (if any). The list is NOT updated if rule Failed.
            currentSkipped : list<string> // Currently skipped words, if any.
            allSkipped : list<string> // All skipped words.
            unprocessed : list<string> // List of remaining (unprocessed) words. The list is NOT updated if rule Failed.
            address : Address // Updated address if rule matches OR input.address if rule failed.
            matchError : MatchError option
        }

        static member create u : MatchInfo =
            {
                result = Perfect
                stepResults = DetailedStepResult.defaultValue
                resolved = []
                currentSkipped = []
                allSkipped = []
                unprocessed = u
                address = Address.defaultValue
                matchError = None
            }

        /// TODO kk:20191105 - This probably needs some tweaks.
        /// We might want to extract what is missing in the invalid addresses (usually that would be missing house number)
        /// and then do something about that. The question is what to do?
        member this.serializeAddresses (useAbbreviatedNames : bool, removeUnit : bool) =
            let toErrorMessage() =
                "Error occured processing the rest."

            let v =
                this.resolved
                |> List.filter (fun e -> e.resolvedAddress.isValid)
                |> List.map (fun a -> None, if useAbbreviatedNames then a.resolvedAddress.asString removeUnit else a.resolvedAddress.asOriginalString removeUnit)

            let x =
                match this.matchError with
                | None -> v
                | _ -> (this.matchError, toErrorMessage()) :: v

            x |> List.toArray


    let rec compareUnitNumbers (a : UnitNumber option) (b : UnitNumber option) =
        let distance u w = levenshtein u w |> MatchResult.Partial

        match a, b with
        | None, None -> MatchResult.Perfect
        | Some u, None -> MatchResult.almostFailing + distance u.value EmptyString
        | None, Some _ -> compareUnitNumbers b a
        | Some u, Some w ->
            match u = w with
            | true -> MatchResult.Perfect
            | false -> distance u.value w.value
