namespace Softellect.AddressProcessor

    //Generated, do not rewrite
    type State =
        | AK
        | AL
        | AR
        | AS
        | AZ
        | CA
        | CO
        | CT
        | DC
        | DE
        | FL
        | FM
        | GA
        | GU
        | HI
        | IA
        | ID
        | IL
        | IN
        | KS
        | KY
        | LA
        | MA
        | MD
        | ME
        | MH
        | MI
        | MN
        | MO
        | MP
        | MS
        | MT
        | NC
        | ND
        | NE
        | NH
        | NJ
        | NM
        | NV
        | NY
        | OH
        | OK
        | OR
        | PA
        | PR
        | PW
        | RI
        | SC
        | SD
        | TN
        | TX
        | UT
        | VA
        | VI
        | VT
        | WA
        | WI
        | WV
        | WY

        member this.value = this.ToString()

        static member all =
            [|
                (State.AK, "Alaska", "AK")
                (State.AL, "Alabama", "AL")
                (State.AR, "Arkansas", "AR")
                (State.AS, "American Samoa", "AS")
                (State.AZ, "Arizona", "AZ")
                (State.CA, "California", "CA")
                (State.CO, "Colorado", "CO")
                (State.CT, "Connecticut", "CT")
                (State.DC, "District of Columbia", "DC")
                (State.DE, "Delaware", "DE")
                (State.FL, "Florida", "FL")
                (State.FM, "Federated States of Micronesia", "FM")
                (State.GA, "Georgia", "GA")
                (State.GU, "Guam", "GU")
                (State.HI, "Hawaii", "HI")
                (State.IA, "Iowa", "IA")
                (State.ID, "Idaho", "ID")
                (State.IL, "Illinois", "IL")
                (State.IN, "Indiana", "IN")
                (State.KS, "Kansas", "KS")
                (State.KY, "Kentucky", "KY")
                (State.LA, "Louisiana", "LA")
                (State.MA, "Massachusetts", "MA")
                (State.MD, "Maryland", "MD")
                (State.ME, "Maine", "ME")
                (State.MH, "Marshall Islands", "MH")
                (State.MI, "Michigan", "MI")
                (State.MN, "Minnesota", "MN")
                (State.MO, "Missouri", "MO")
                (State.MP, "Northern Marianas", "MP")
                (State.MS, "Mississippi", "MS")
                (State.MT, "Montana", "MT")
                (State.NC, "North Carolina", "NC")
                (State.ND, "North Dakota", "ND")
                (State.NE, "Nebraska", "NE")
                (State.NH, "New Hampshire", "NH")
                (State.NJ, "New Jersey", "NJ")
                (State.NM, "New Mexico", "NM")
                (State.NV, "Nevada", "NV")
                (State.NY, "New York", "NY")
                (State.OH, "Ohio", "OH")
                (State.OK, "Oklahoma", "OK")
                (State.OR, "Oregon", "OR")
                (State.PA, "Pennsylvania", "PA")
                (State.PR, "Puerto Rico", "PR")
                (State.PW, "Palau", "PW")
                (State.RI, "Rhode Island", "RI")
                (State.SC, "South Carolina", "SC")
                (State.SD, "South Dakota", "SD")
                (State.TN, "Tennessee", "TN")
                (State.TX, "Texas", "TX")
                (State.UT, "Utah", "UT")
                (State.VA, "Virginia", "VA")
                (State.VI, "Virgin Islands", "VI")
                (State.VT, "Vermont", "VT")
                (State.WA, "Washington", "WA")
                (State.WI, "Wisconsin", "WI")
                (State.WV, "West Virginia", "WV")
                (State.WY, "Wyoming", "WY")
            |]


    type StateFactory () = inherit UnionFactory<State, string, string> (State.all)
