namespace Softellect.AddressProcessor

open CSharpInterop
open MetricTreeInterop

module MatchParams =

    /// Default distance to use in fuzzy street searches.
    let streetDefaultDistance (s : string) =
        match s.Length with
        | 0 | 1 | 2 -> 1
        | 3 | 4 | 5 -> 2
        | 6 | 7 | 8 | 9 -> 3
        | _ -> 4


    /// Default distance to use when a better street match is needed due to some reasons.
    let streetStrictDistance (s : string) =
        match s.Length with
        | 0 | 1 | 2 -> 0
        | 3 | 4 | 5 -> 1
        | 6 | 7 | 8 | 9 -> 2
        | _ -> 3


    /// Default distance to use in fuzzy city searches.
    let cityDefaultDistance (s : string) =
        match s.Length with
        | 0 | 1 | 2 -> 1
        | 3 | 4 | 5 -> 2
        | 6 | 7 | 8 | 9 -> 3
        | _ -> 4


    type FuzzySearchType =
        | ExactSearch
        | FuzzySearch


    type FuzzyBaseComparisionType =
        | CompareLists
        | NormalizedDistance
        | NormalizedDistWithProjection


    type FuzzyComparisionType =
        | BestOf of FuzzyBaseComparisionType * list<FuzzyBaseComparisionType>
        | AverageOf of FuzzyBaseComparisionType * list<FuzzyBaseComparisionType>
        | AverageNotFailedOf of FuzzyBaseComparisionType * list<FuzzyBaseComparisionType>


        static member defaultStreetValue = AverageNotFailedOf (NormalizedDistance, [ CompareLists; NormalizedDistWithProjection ])
        static member defaultStreetProjector = removeVowelsProjector

        static member defaultCityValue = AverageNotFailedOf (NormalizedDistance, [ CompareLists; NormalizedDistWithProjection ])
        static member defaultCityProjector = removeVowelsProjector


    type StreetSearchParams =
        {
            streetSearchType : FuzzySearchType
            streetComparisionTypeOpt : FuzzyComparisionType option
            streetProjectorOpt : (string -> string) option
            streetDistanceOpt : (string -> int) option
        }


    type CitySearchParams =
        {
            citySearchType : FuzzySearchType
            cityComparisionTypeOpt : FuzzyComparisionType option
            cityProjectorOpt : (string -> string) option
            cityDistanceOpt : (string -> int) option
        }


    type FuzzySearchParams =
        {
            streetSearchParams : StreetSearchParams
            citySearchParams : CitySearchParams
        }

        member p.streetComparisionType = Option.defaultValue FuzzyComparisionType.defaultStreetValue p.streetSearchParams.streetComparisionTypeOpt
        member p.streetProjector = Option.defaultValue FuzzyComparisionType.defaultStreetProjector p.streetSearchParams.streetProjectorOpt
        member p.streetDistance = Option.defaultValue streetDefaultDistance p.streetSearchParams.streetDistanceOpt

        member p.cityComparisionType = Option.defaultValue FuzzyComparisionType.defaultCityValue p.citySearchParams.cityComparisionTypeOpt
        member p.cityProjector = Option.defaultValue FuzzyComparisionType.defaultCityProjector p.citySearchParams.cityProjectorOpt
        member p.cityDistance = Option.defaultValue cityDefaultDistance p.citySearchParams.cityDistanceOpt

        static member exactSearchValue =
            {
                streetSearchParams =
                    {
                        streetSearchType = ExactSearch
                        streetComparisionTypeOpt = (CompareLists, []) |> BestOf |> Some
                        streetProjectorOpt = None
                        streetDistanceOpt = None
                    }

                citySearchParams =
                    {
                        citySearchType = ExactSearch
                        cityComparisionTypeOpt = (CompareLists, []) |> BestOf |> Some
                        cityProjectorOpt = None
                        cityDistanceOpt = None
                    }
            }

        /// Use when performing exact search but skipping the city.
        /// We must have stricter street comparison rules if we skip the city.
        static member exactSearchWithSkippedCityValue =
            {
                streetSearchParams =
                    {
                        streetSearchType = ExactSearch
                        streetComparisionTypeOpt = (NormalizedDistance, []) |> BestOf |> Some
                        streetProjectorOpt = Some id
                        streetDistanceOpt = Some streetStrictDistance
                    }

                citySearchParams =
                    {
                        citySearchType = ExactSearch
                        cityComparisionTypeOpt = (CompareLists, []) |> BestOf |> Some
                        cityProjectorOpt = None
                        cityDistanceOpt = None
                    }
            }

        static member fuzzySearchValue =
            {
                streetSearchParams =
                    {
                        streetSearchType = FuzzySearch
                        streetComparisionTypeOpt = None
                        streetProjectorOpt = None
                        streetDistanceOpt = None
                    }

                citySearchParams =
                    {
                        citySearchType = FuzzySearch
                        cityComparisionTypeOpt = None
                        cityProjectorOpt = None
                        cityDistanceOpt = None
                    }
            }


        static member defaultValue = FuzzySearchParams.exactSearchValue


    /// Various general weights and other parameters related to skipping words.
    type SkipParams =
        {
            skippedWordWeight : float // Weight of a skipped word. Weight of unresolved word is always 1.0.
            matchWeight : float // Weight of a match score.
            roundingMultiplier : float // Multiplier to round the weights.
        }

        static member strictValue =
            {
                skippedWordWeight = 5.0
                matchWeight = 1.0
                roundingMultiplier = 1_000.0
            }

        static member mediumValue =
            {
                skippedWordWeight = 3.0
                matchWeight = 1.0
                roundingMultiplier = 1_000.0
            }

        static member relaxedValue =
            {
                skippedWordWeight = 1.5
                matchWeight = 1.0
                roundingMultiplier = 1_000.0
            }


    type CityRulesParams =
        {
            maxUnitNumberSkip : int
        }

        static member strictValue =
            {
                maxUnitNumberSkip = 1
            }

        static member mediumValue =
            {
                maxUnitNumberSkip = 2
            }

        static member relaxedValue =
            {
                maxUnitNumberSkip = 3
            }


    type MatchParams =
        {
            cityRulesParams : CityRulesParams
            skipParams : SkipParams
            fuzzySearchParams : FuzzySearchParams
        }

        static member strictValue =
            {
                cityRulesParams = CityRulesParams.strictValue
                skipParams = SkipParams.strictValue
                fuzzySearchParams = FuzzySearchParams.exactSearchValue
            }

        /// Use when skipping the city.
        static member strictSkippedCityValue =
            {
                cityRulesParams = CityRulesParams.strictValue
                skipParams = SkipParams.strictValue
                fuzzySearchParams = FuzzySearchParams.exactSearchWithSkippedCityValue
            }

        static member strictFuzzySearchValue =
            {
                cityRulesParams = CityRulesParams.strictValue
                skipParams = SkipParams.strictValue
                fuzzySearchParams = FuzzySearchParams.fuzzySearchValue
            }

        static member mediumValue =
            {
                cityRulesParams = CityRulesParams.mediumValue
                skipParams = SkipParams.mediumValue
                fuzzySearchParams = FuzzySearchParams.exactSearchValue
            }

        static member relaxedValue =
            {
                cityRulesParams = CityRulesParams.relaxedValue
                skipParams = SkipParams.relaxedValue
                fuzzySearchParams = FuzzySearchParams.exactSearchValue
            }
