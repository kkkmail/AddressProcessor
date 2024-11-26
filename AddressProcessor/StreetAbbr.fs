namespace Softellect.AddressProcessor

// Generated, do not modify
// See StreetAbbr__<YYYYMMDD>.xlsx

type StreetType =
    | Aly
    | Anx
    | Arc
    | Ave
    | Bay // Manually added !!!
    | Bch
    | Bde // Manually added !!!
    | Bg
    | Bgs
    | Blf
    | Blfs
    | Blvd
    | Bnd
    | Br
    | Brdwy // USPS, manually added !!!
    | Brg
    | Brk
    | Brks
    | Btm
    | Byp
    | Byu
    | Cir
    | Cirs
    | Clb
    | Clf
    | Clfs

    | Close // Another gift from Marietta, GA

    | Cmn
    | Cmns
    | Cor
    | Cors
    | Cp
    | Cpe
    | Cres
    | Crk
    | Crse
    | Crst
    | Cswy
    | Ct
    | Ctr
    | Ctrs
    | Cts
    | Curv
    | Cv
    | Cvs
    | Cyn
    | Dl
    | Dm
    | Dr
    | Drs
    | Dv

    | End // "LANDEVEIS END, MARIETTA, GA" fix

    | Est
    | Ests
    | Expy
    | Ext
    | Exts
    | Fall
    | Fld
    | Flds
    | Fls
    | Flt
    | Flts
    | Frd
    | Frds
    | Frg
    | Frgs
    | Frk
    | Frks
    | Frst
    | Fry
    | Ft
    | Fwy
    | Gdn
    | Gdns
    | Gln
    | Glns
    | Grn
    | Grns
    | Grv
    | Grvs
    | Gtwy
    | Hbr
    | Hbrs
    | Hl
    | Hls
    | Holw
    | Hts
    | Hvn
    | Hwy
    | Inlt
    | Is
    | Isle
    | Iss
    | Jct
    | Jcts
    | Knl
    | Knls
    | Ky
    | Kys
    | Land
    | Lck
    | Lcks
    | Ldg
    | Lf
    | Lgt
    | Lgts
    | Lk
    | Lks
    | Ln
    | Lndg
    | Loop
    | Mall
    | Mdw
    | Mdws
    | Mews
    | Ml
    | Mls
    | Mnr
    | Mnrs
    | Msn
    | Mt
    | Mtn
    | Mtns
    | Mtwy
    | Nck
    | Opas
    | Orch
    | Oval
    | Park
    | Pass
    | Path
    | Pike
    | Pkwy
    | Pl
    | Pln
    | Plns
    | Plz
    | Pne
    | Pnes
    | Pr
    | Prt
    | Prts
    | Psge
    | Pt
    | Pts
    | Radl
    | Ramp
    | Rd
    | Rdg
    | Rdgs
    | Rds
    | Riv
    | Rnch
    | Row
    | Rpd
    | Rpds
    | Rst
    | Rte
    | Rue
    | Run
    | Shl
    | Shls
    | Shr
    | Shrs
    | Skwy
    | Smt
    | Spg
    | Spgs
    | Spur
    | Sq
    | Sqs
    | St
    | Sta
    | Stra
    | Strm
    | Sts
    | Ter
    | Tpke
    | Trak
    | Trce
    | Trfy
    | Trl

    // kk:20191022 - MD4 has only 14 addresses with the street type "trailer" but over 95K addresses with "trailer" as a suite name.
    //| Trlr

    | Trwy
    | Tunl
    | Un
    | Uns
    | Upas
    | Via
    | Vis
    | Vl
    | Vlg
    | Vlgs
    | Vly
    | Vlys
    | Vw
    | Vws
    | Walk
    | Wall
    | Way
    | Ways
    | Wl
    | Wls
    | Xing
    | Xrd
    | Xrds

    // kk:20191209 - The street types below are manually added Spanish / French equivalents of street types. Keep adding as needed.
    // Spanish
    | Avenida
    | Calle

    // French
with
    static member all =
        [|
            ( StreetType.Aly, "ALY" )
            ( StreetType.Anx, "ANX" )
            ( StreetType.Arc, "ARC" )
            ( StreetType.Ave, "AVE" )

            ( StreetType.Bay, "BAY" ) // Manually added

            ( StreetType.Bch, "BCH" )
            ( StreetType.Bde, "BDE" ) // Manually added
            ( StreetType.Bg, "BG" )
            ( StreetType.Bgs, "BGS" )
            ( StreetType.Blf, "BLF" )
            ( StreetType.Blfs, "BLFS" )
            ( StreetType.Blvd, "BLVD" )
            ( StreetType.Bnd, "BND" )
            ( StreetType.Br, "BR" )

            ( StreetType.Brdwy, "BRDWY" ) // Manually added

            ( StreetType.Brg, "BRG" )
            ( StreetType.Brk, "BRK" )
            ( StreetType.Brks, "BRKS" )
            ( StreetType.Btm, "BTM" )
            ( StreetType.Byp, "BYP" )
            ( StreetType.Byu, "BYU" )
            ( StreetType.Cir, "CIR" )
            ( StreetType.Cirs, "CIRS" )
            ( StreetType.Clb, "CLB" )
            ( StreetType.Clf, "CLF" )
            ( StreetType.Clfs, "CLFS" )

            // Marietta, GA, ...
            ( StreetType.Close, "CLOSE" )

            ( StreetType.Cmn, "CMN" )
            ( StreetType.Cmns, "CMNS" )
            ( StreetType.Cor, "COR" )
            ( StreetType.Cors, "CORS" )
            ( StreetType.Cp, "CP" )
            ( StreetType.Cpe, "CPE" )
            ( StreetType.Cres, "CRES" )
            ( StreetType.Crk, "CRK" )
            ( StreetType.Crse, "CRSE" )
            ( StreetType.Crst, "CRST" )
            ( StreetType.Cswy, "CSWY" )
            ( StreetType.Ct, "CT" )
            ( StreetType.Ctr, "CTR" )
            ( StreetType.Ctrs, "CTRS" )
            ( StreetType.Cts, "CTS" )
            ( StreetType.Curv, "CURV" )
            ( StreetType.Cv, "CV" )
            ( StreetType.Cvs, "CVS" )
            ( StreetType.Cyn, "CYN" )
            ( StreetType.Dl, "DL" )
            ( StreetType.Dm, "DM" )
            ( StreetType.Dr, "DR" )
            ( StreetType.Drs, "DRS" )
            ( StreetType.Dv, "DV" )

            // !!! Welcome to "LANDEVEIS END, MARIETTA, GA !!!"
            ( StreetType.End, "END" )

            ( StreetType.Est, "EST" )
            ( StreetType.Ests, "ESTS" )
            ( StreetType.Expy, "EXPY" )
            ( StreetType.Ext, "EXT" )
            ( StreetType.Exts, "EXTS" )
            ( StreetType.Fall, "FALL" )
            ( StreetType.Fld, "FLD" )
            ( StreetType.Flds, "FLDS" )
            ( StreetType.Fls, "FLS" )
            ( StreetType.Flt, "FLT" )
            ( StreetType.Flts, "FLTS" )
            ( StreetType.Frd, "FRD" )
            ( StreetType.Frds, "FRDS" )
            ( StreetType.Frg, "FRG" )
            ( StreetType.Frgs, "FRGS" )
            ( StreetType.Frk, "FRK" )
            ( StreetType.Frks, "FRKS" )
            ( StreetType.Frst, "FRST" )
            ( StreetType.Fry, "FRY" )
            ( StreetType.Ft, "FT" )
            ( StreetType.Fwy, "FWY" )
            ( StreetType.Gdn, "GDN" )
            ( StreetType.Gdns, "GDNS" )
            ( StreetType.Gln, "GLN" )
            ( StreetType.Glns, "GLNS" )
            ( StreetType.Grn, "GRN" )
            ( StreetType.Grns, "GRNS" )
            ( StreetType.Grv, "GRV" )
            ( StreetType.Grvs, "GRVS" )
            ( StreetType.Gtwy, "GTWY" )
            ( StreetType.Hbr, "HBR" )
            ( StreetType.Hbrs, "HBRS" )
            ( StreetType.Hl, "HL" )
            ( StreetType.Hls, "HLS" )
            ( StreetType.Holw, "HOLW" )
            ( StreetType.Hts, "HTS" )
            ( StreetType.Hvn, "HVN" )
            ( StreetType.Hwy, "HWY" )
            ( StreetType.Inlt, "INLT" )
            ( StreetType.Is, "IS" )
            ( StreetType.Isle, "ISLE" )
            ( StreetType.Iss, "ISS" )
            ( StreetType.Jct, "JCT" )
            ( StreetType.Jcts, "JCTS" )
            ( StreetType.Knl, "KNL" )
            ( StreetType.Knls, "KNLS" )
            ( StreetType.Ky, "KY" )
            ( StreetType.Kys, "KYS" )
            ( StreetType.Land, "LAND" )
            ( StreetType.Lck, "LCK" )
            ( StreetType.Lcks, "LCKS" )
            ( StreetType.Ldg, "LDG" )
            ( StreetType.Lf, "LF" )
            ( StreetType.Lgt, "LGT" )
            ( StreetType.Lgts, "LGTS" )
            ( StreetType.Lk, "LK" )
            ( StreetType.Lks, "LKS" )
            ( StreetType.Ln, "LN" )
            ( StreetType.Lndg, "LNDG" )
            ( StreetType.Loop, "LOOP" )
            ( StreetType.Mall, "MALL" )
            ( StreetType.Mdw, "MDW" )
            ( StreetType.Mdws, "MDWS" )
            ( StreetType.Mews, "MEWS" )
            ( StreetType.Ml, "ML" )
            ( StreetType.Mls, "MLS" )
            ( StreetType.Mnr, "MNR" )
            ( StreetType.Mnrs, "MNRS" )
            ( StreetType.Msn, "MSN" )
            ( StreetType.Mt, "MT" )
            ( StreetType.Mtn, "MTN" )
            ( StreetType.Mtns, "MTNS" )
            ( StreetType.Mtwy, "MTWY" )
            ( StreetType.Nck, "NCK" )
            ( StreetType.Opas, "OPAS" )
            ( StreetType.Orch, "ORCH" )
            ( StreetType.Oval, "OVAL" )
            ( StreetType.Park, "PARK" )
            ( StreetType.Pass, "PASS" )
            ( StreetType.Path, "PATH" )
            ( StreetType.Pike, "PIKE" )
            ( StreetType.Pkwy, "PKWY" )
            ( StreetType.Pl, "PL" )
            ( StreetType.Pln, "PLN" )
            ( StreetType.Plns, "PLNS" )
            ( StreetType.Plz, "PLZ" )
            ( StreetType.Pne, "PNE" )
            ( StreetType.Pnes, "PNES" )
            ( StreetType.Pr, "PR" )
            ( StreetType.Prt, "PRT" )
            ( StreetType.Prts, "PRTS" )
            ( StreetType.Psge, "PSGE" )
            ( StreetType.Pt, "PT" )
            ( StreetType.Pts, "PTS" )
            ( StreetType.Radl, "RADL" )
            ( StreetType.Ramp, "RAMP" )
            ( StreetType.Rd, "RD" )
            ( StreetType.Rdg, "RDG" )
            ( StreetType.Rdgs, "RDGS" )
            ( StreetType.Rds, "RDS" )
            ( StreetType.Riv, "RIV" )
            ( StreetType.Rnch, "RNCH" )
            ( StreetType.Row, "ROW" )
            ( StreetType.Rpd, "RPD" )
            ( StreetType.Rpds, "RPDS" )
            ( StreetType.Rst, "RST" )
            ( StreetType.Rte, "RTE" )
            ( StreetType.Rue, "RUE" )
            ( StreetType.Run, "RUN" )
            ( StreetType.Shl, "SHL" )
            ( StreetType.Shls, "SHLS" )
            ( StreetType.Shr, "SHR" )
            ( StreetType.Shrs, "SHRS" )
            ( StreetType.Skwy, "SKWY" )
            ( StreetType.Smt, "SMT" )
            ( StreetType.Spg, "SPG" )
            ( StreetType.Spgs, "SPGS" )
            ( StreetType.Spur, "SPUR" )
            ( StreetType.Sq, "SQ" )
            ( StreetType.Sqs, "SQS" )
            ( StreetType.St, "ST" )
            ( StreetType.Sta, "STA" )
            ( StreetType.Stra, "STRA" )
            ( StreetType.Strm, "STRM" )
            ( StreetType.Sts, "STS" )
            ( StreetType.Ter, "TER" )
            ( StreetType.Tpke, "TPKE" )
            ( StreetType.Trak, "TRAK" )
            ( StreetType.Trce, "TRCE" )
            ( StreetType.Trfy, "TRFY" )
            ( StreetType.Trl, "TRL" )

            // kk:20191022 - MD4 has only 14 addresses with the street type "trailer" but over 95K addresses with "trailer" as a suite name.
            //( StreetType.Trlr, "TRLR" )

            ( StreetType.Trwy, "TRWY" )
            ( StreetType.Tunl, "TUNL" )
            ( StreetType.Un, "UN" )
            ( StreetType.Uns, "UNS" )
            ( StreetType.Upas, "UPAS" )
            ( StreetType.Via, "VIA" )
            ( StreetType.Vis, "VIS" )
            ( StreetType.Vl, "VL" )
            ( StreetType.Vlg, "VLG" )
            ( StreetType.Vlgs, "VLGS" )
            ( StreetType.Vly, "VLY" )
            ( StreetType.Vlys, "VLYS" )
            ( StreetType.Vw, "VW" )
            ( StreetType.Vws, "VWS" )
            ( StreetType.Walk, "WALK" )
            ( StreetType.Wall, "WALL" )
            ( StreetType.Way, "WAY" )
            ( StreetType.Ways, "WAYS" )
            ( StreetType.Wl, "WL" )
            ( StreetType.Wls, "WLS" )
            ( StreetType.Xing, "XING" )
            ( StreetType.Xrd, "XRD" )
            ( StreetType.Xrds, "XRDS" )

            // kk:20191209 - The street types below are manually added Spanish / French equivalents of street types. Keep adding as needed.
            // Spanish
            ( StreetType.Avenida, "AVENIDA" )
            ( StreetType.Calle, "CALLE" )

            // French
        |]


type StreetTypeFactory () = inherit UnionFactory<StreetType, string> (StreetType.all)


// TODO Look at https://pe.usps.com/text/pub28/28apg.htm
type StreetAbbr =
    {
        primary : string
        common : string
        standard : string
        canBeMid : bool
        caseValue : StreetType
    }
with
    static member all =
        [|
            { primary = "ALLEY"; common = "ALLEE"; standard= "ALY"; canBeMid = false; caseValue = StreetType.Aly }
            { primary = "ALLEY"; common = "ALLEY"; standard= "ALY"; canBeMid = false; caseValue = StreetType.Aly }
            { primary = "ALLEY"; common = "ALLY"; standard= "ALY"; canBeMid = false; caseValue = StreetType.Aly }
            { primary = "ALLEY"; common = "ALY"; standard= "ALY"; canBeMid = false; caseValue = StreetType.Aly }
            { primary = "ANEX"; common = "ANEX"; standard= "ANX"; canBeMid = false; caseValue = StreetType.Anx }
            { primary = "ANEX"; common = "ANNEX"; standard= "ANX"; canBeMid = false; caseValue = StreetType.Anx }
            { primary = "ANEX"; common = "ANNX"; standard= "ANX"; canBeMid = false; caseValue = StreetType.Anx }
            { primary = "ANEX"; common = "ANX"; standard= "ANX"; canBeMid = false; caseValue = StreetType.Anx }
            { primary = "ARCADE"; common = "ARC"; standard= "ARC"; canBeMid = false; caseValue = StreetType.Arc }
            { primary = "ARCADE"; common = "ARCADE"; standard= "ARC"; canBeMid = false; caseValue = StreetType.Arc }
            { primary = "AVENUE"; common = "AV"; standard= "AVE"; canBeMid = false; caseValue = StreetType.Ave }
            { primary = "AVENUE"; common = "AVE"; standard= "AVE"; canBeMid = false; caseValue = StreetType.Ave }
            { primary = "AVENUE"; common = "AVEN"; standard= "AVE"; canBeMid = false; caseValue = StreetType.Ave }
            { primary = "AVENUE"; common = "AVENU"; standard= "AVE"; canBeMid = false; caseValue = StreetType.Ave }
            { primary = "AVENUE"; common = "AVENUE"; standard= "AVE"; canBeMid = false; caseValue = StreetType.Ave }
            { primary = "AVENUE"; common = "AVN"; standard= "AVE"; canBeMid = false; caseValue = StreetType.Ave }
            { primary = "AVENUE"; common = "AVNUE"; standard= "AVE"; canBeMid = false; caseValue = StreetType.Ave }

            // Weird street type about which even post office does not know.
            { primary = "BAY"; common = "BAY"; standard= "BAY"; canBeMid = false; caseValue = StreetType.Bay }

            { primary = "BAYOU"; common = "BAYOO"; standard= "BYU"; canBeMid = false; caseValue = StreetType.Byu }
            { primary = "BAYOU"; common = "BAYOU"; standard= "BYU"; canBeMid = false; caseValue = StreetType.Byu }
            { primary = "BEACH"; common = "BCH"; standard= "BCH"; canBeMid = false; caseValue = StreetType.Bch }
            { primary = "BEACH"; common = "BEACH"; standard= "BCH"; canBeMid = false; caseValue = StreetType.Bch }
            { primary = "BEND"; common = "BEND"; standard= "BND"; canBeMid = false; caseValue = StreetType.Bnd }
            { primary = "BEND"; common = "BND"; standard= "BND"; canBeMid = false; caseValue = StreetType.Bnd }
            { primary = "BLUFF"; common = "BLF"; standard= "BLF"; canBeMid = false; caseValue = StreetType.Blf }
            { primary = "BLUFF"; common = "BLUF"; standard= "BLF"; canBeMid = false; caseValue = StreetType.Blf }
            { primary = "BLUFF"; common = "BLUFF"; standard= "BLF"; canBeMid = false; caseValue = StreetType.Blf }
            { primary = "BLUFFS"; common = "BLUFFS"; standard= "BLFS"; canBeMid = false; caseValue = StreetType.Blfs }
            { primary = "BOTTOM"; common = "BOT"; standard= "BTM"; canBeMid = false; caseValue = StreetType.Btm }
            { primary = "BOTTOM"; common = "BTM"; standard= "BTM"; canBeMid = false; caseValue = StreetType.Btm }
            { primary = "BOTTOM"; common = "BOTTM"; standard= "BTM"; canBeMid = false; caseValue = StreetType.Btm }
            { primary = "BOTTOM"; common = "BOTTOM"; standard= "BTM"; canBeMid = false; caseValue = StreetType.Btm }
            { primary = "BOULEVARD"; common = "BLVD"; standard= "BLVD"; canBeMid = false; caseValue = StreetType.Blvd }
            { primary = "BOULEVARD"; common = "BOUL"; standard= "BLVD"; canBeMid = false; caseValue = StreetType.Blvd }
            { primary = "BOULEVARD"; common = "BOULEVARD"; standard= "BLVD"; canBeMid = false; caseValue = StreetType.Blvd }
            { primary = "BOULEVARD"; common = "BOULV"; standard= "BLVD"; canBeMid = false; caseValue = StreetType.Blvd }
            { primary = "BRANCH"; common = "BR"; standard= "BR"; canBeMid = false; caseValue = StreetType.Br }
            { primary = "BRANCH"; common = "BRNCH"; standard= "BR"; canBeMid = false; caseValue = StreetType.Br }
            { primary = "BRANCH"; common = "BRANCH"; standard= "BR"; canBeMid = false; caseValue = StreetType.Br }
            { primary = "BRIDGE"; common = "BRDGE"; standard= "BRG"; canBeMid = false; caseValue = StreetType.Brg }
            { primary = "BRIDGE"; common = "BRG"; standard= "BRG"; canBeMid = false; caseValue = StreetType.Brg }
            { primary = "BRIDGE"; common = "BRIDGE"; standard= "BRG"; canBeMid = false; caseValue = StreetType.Brg }

            // Manually added
            { primary = "BRIGADE"; common = "BRIGADE"; standard= "BDE"; canBeMid = false; caseValue = StreetType.Bde }

            // Manually added
            { primary = "BROADWAY"; common = "BROADWAY"; standard= "BRDWY"; canBeMid = false; caseValue = StreetType.Brdwy }

            { primary = "BROOK"; common = "BRK"; standard= "BRK"; canBeMid = false; caseValue = StreetType.Brk }
            { primary = "BROOK"; common = "BROOK"; standard= "BRK"; canBeMid = false; caseValue = StreetType.Brk }
            { primary = "BROOKS"; common = "BROOKS"; standard= "BRKS"; canBeMid = false; caseValue = StreetType.Brks }
            { primary = "BURG"; common = "BURG"; standard= "BG"; canBeMid = false; caseValue = StreetType.Bg }
            { primary = "BURGS"; common = "BURGS"; standard= "BGS"; canBeMid = false; caseValue = StreetType.Bgs }
            { primary = "BYPASS"; common = "BYP"; standard= "BYP"; canBeMid = false; caseValue = StreetType.Byp }
            { primary = "BYPASS"; common = "BYPA"; standard= "BYP"; canBeMid = false; caseValue = StreetType.Byp }
            { primary = "BYPASS"; common = "BYPAS"; standard= "BYP"; canBeMid = false; caseValue = StreetType.Byp }
            { primary = "BYPASS"; common = "BYPASS"; standard= "BYP"; canBeMid = false; caseValue = StreetType.Byp }
            { primary = "BYPASS"; common = "BYPS"; standard= "BYP"; canBeMid = false; caseValue = StreetType.Byp }
            { primary = "CAMP"; common = "CAMP"; standard= "CP"; canBeMid = false; caseValue = StreetType.Cp }
            { primary = "CAMP"; common = "CP"; standard= "CP"; canBeMid = false; caseValue = StreetType.Cp }
            { primary = "CAMP"; common = "CMP"; standard= "CP"; canBeMid = false; caseValue = StreetType.Cp }
            { primary = "CANYON"; common = "CANYN"; standard= "CYN"; canBeMid = false; caseValue = StreetType.Cyn }
            { primary = "CANYON"; common = "CANYON"; standard= "CYN"; canBeMid = false; caseValue = StreetType.Cyn }
            { primary = "CANYON"; common = "CNYN"; standard= "CYN"; canBeMid = false; caseValue = StreetType.Cyn }
            { primary = "CAPE"; common = "CAPE"; standard= "CPE"; canBeMid = false; caseValue = StreetType.Cpe }
            { primary = "CAPE"; common = "CPE"; standard= "CPE"; canBeMid = false; caseValue = StreetType.Cpe }
            { primary = "CAUSEWAY"; common = "CAUSEWAY"; standard= "CSWY"; canBeMid = false; caseValue = StreetType.Cswy }
            { primary = "CAUSEWAY"; common = "CAUSWA"; standard= "CSWY"; canBeMid = false; caseValue = StreetType.Cswy }
            { primary = "CAUSEWAY"; common = "CSWY"; standard= "CSWY"; canBeMid = false; caseValue = StreetType.Cswy }
            { primary = "CENTER"; common = "CEN"; standard= "CTR"; canBeMid = false; caseValue = StreetType.Ctr }
            { primary = "CENTER"; common = "CENT"; standard= "CTR"; canBeMid = false; caseValue = StreetType.Ctr }
            { primary = "CENTER"; common = "CENTER"; standard= "CTR"; canBeMid = false; caseValue = StreetType.Ctr }
            { primary = "CENTER"; common = "CENTR"; standard= "CTR"; canBeMid = false; caseValue = StreetType.Ctr }
            { primary = "CENTER"; common = "CENTRE"; standard= "CTR"; canBeMid = false; caseValue = StreetType.Ctr }
            { primary = "CENTER"; common = "CNTER"; standard= "CTR"; canBeMid = false; caseValue = StreetType.Ctr }
            { primary = "CENTER"; common = "CNTR"; standard= "CTR"; canBeMid = false; caseValue = StreetType.Ctr }
            { primary = "CENTER"; common = "CTR"; standard= "CTR"; canBeMid = false; caseValue = StreetType.Ctr }
            { primary = "CENTERS"; common = "CENTERS"; standard= "CTRS"; canBeMid = false; caseValue = StreetType.Ctrs }
            { primary = "CIRCLE"; common = "CIR"; standard= "CIR"; canBeMid = false; caseValue = StreetType.Cir }
            { primary = "CIRCLE"; common = "CIRC"; standard= "CIR"; canBeMid = false; caseValue = StreetType.Cir }
            { primary = "CIRCLE"; common = "CIRCL"; standard= "CIR"; canBeMid = false; caseValue = StreetType.Cir }
            { primary = "CIRCLE"; common = "CIRCLE"; standard= "CIR"; canBeMid = false; caseValue = StreetType.Cir }
            { primary = "CIRCLE"; common = "CRCL"; standard= "CIR"; canBeMid = false; caseValue = StreetType.Cir }
            { primary = "CIRCLE"; common = "CRCLE"; standard= "CIR"; canBeMid = false; caseValue = StreetType.Cir }
            { primary = "CIRCLES"; common = "CIRCLES"; standard= "CIRS"; canBeMid = false; caseValue = StreetType.Cirs }
            { primary = "CLIFF"; common = "CLF"; standard= "CLF"; canBeMid = false; caseValue = StreetType.Clf }
            { primary = "CLIFF"; common = "CLIFF"; standard= "CLF"; canBeMid = false; caseValue = StreetType.Clf }
            { primary = "CLIFFS"; common = "CLFS"; standard= "CLFS"; canBeMid = false; caseValue = StreetType.Clfs }
            { primary = "CLIFFS"; common = "CLIFFS"; standard= "CLFS"; canBeMid = false; caseValue = StreetType.Clfs }
            { primary = "CLUB"; common = "CLB"; standard= "CLB"; canBeMid = false; caseValue = StreetType.Clb }

            // Marietta, GA ...
            { primary = "CLOSE"; common = "CLOSE"; standard= "CLOSE"; canBeMid = false; caseValue = StreetType.Close }

            { primary = "CLUB"; common = "CLUB"; standard= "CLB"; canBeMid = false; caseValue = StreetType.Clb }
            { primary = "COMMON"; common = "COMMON"; standard= "CMN"; canBeMid = false; caseValue = StreetType.Cmn }

            // Mistype
            { primary = "CM"; common = "CM"; standard= "CMN"; canBeMid = false; caseValue = StreetType.Cmn }

            { primary = "COMMONS"; common = "COMMONS"; standard= "CMNS"; canBeMid = false; caseValue = StreetType.Cmns }
            { primary = "CORNER"; common = "COR"; standard= "COR"; canBeMid = false; caseValue = StreetType.Cor }
            { primary = "CORNER"; common = "CORNER"; standard= "COR"; canBeMid = false; caseValue = StreetType.Cor }
            { primary = "CORNERS"; common = "CORNERS"; standard= "CORS"; canBeMid = false; caseValue = StreetType.Cors }
            { primary = "CORNERS"; common = "CORS"; standard= "CORS"; canBeMid = false; caseValue = StreetType.Cors }
            { primary = "COURSE"; common = "COURSE"; standard= "CRSE"; canBeMid = false; caseValue = StreetType.Crse }
            { primary = "COURSE"; common = "CRSE"; standard= "CRSE"; canBeMid = false; caseValue = StreetType.Crse }
            { primary = "COURT"; common = "COURT"; standard= "CT"; canBeMid = false; caseValue = StreetType.Ct }
            { primary = "COURT"; common = "CT"; standard= "CT"; canBeMid = false; caseValue = StreetType.Ct }
            { primary = "COURTS"; common = "COURTS"; standard= "CTS"; canBeMid = false; caseValue = StreetType.Cts }
            { primary = "COURTS"; common = "CTS"; standard= "CTS"; canBeMid = false; caseValue = StreetType.Cts }
            { primary = "COVE"; common = "COVE"; standard= "CV"; canBeMid = false; caseValue = StreetType.Cv }
            { primary = "COVE"; common = "CV"; standard= "CV"; canBeMid = false; caseValue = StreetType.Cv }
            { primary = "COVES"; common = "COVES"; standard= "CVS"; canBeMid = false; caseValue = StreetType.Cvs }
            { primary = "CREEK"; common = "CREEK"; standard= "CRK"; canBeMid = false; caseValue = StreetType.Crk }
            { primary = "CREEK"; common = "CRK"; standard= "CRK"; canBeMid = false; caseValue = StreetType.Crk }
            { primary = "CRESCENT"; common = "CRESCENT"; standard= "CRES"; canBeMid = false; caseValue = StreetType.Cres }
            { primary = "CRESCENT"; common = "CRES"; standard= "CRES"; canBeMid = false; caseValue = StreetType.Cres }
            { primary = "CRESCENT"; common = "CRSENT"; standard= "CRES"; canBeMid = false; caseValue = StreetType.Cres }
            { primary = "CRESCENT"; common = "CRSNT"; standard= "CRES"; canBeMid = false; caseValue = StreetType.Cres }
            { primary = "CREST"; common = "CREST"; standard= "CRST"; canBeMid = false; caseValue = StreetType.Crst }
            { primary = "CROSSING"; common = "CROSSING"; standard= "XING"; canBeMid = false; caseValue = StreetType.Xing }
            { primary = "CROSSING"; common = "CRSSNG"; standard= "XING"; canBeMid = false; caseValue = StreetType.Xing }
            { primary = "CROSSING"; common = "XING"; standard= "XING"; canBeMid = false; caseValue = StreetType.Xing }
            { primary = "CROSSROAD"; common = "CROSSROAD"; standard= "XRD"; canBeMid = false; caseValue = StreetType.Xrd }
            { primary = "CROSSROADS"; common = "CROSSROADS"; standard= "XRDS"; canBeMid = false; caseValue = StreetType.Xrds }
            { primary = "CURVE"; common = "CURVE"; standard= "CURV"; canBeMid = false; caseValue = StreetType.Curv }
            { primary = "DALE"; common = "DALE"; standard= "DL"; canBeMid = false; caseValue = StreetType.Dl }
            { primary = "DALE"; common = "DL"; standard= "DL"; canBeMid = false; caseValue = StreetType.Dl }
            { primary = "DAM"; common = "DAM"; standard= "DM"; canBeMid = false; caseValue = StreetType.Dm }
            { primary = "DAM"; common = "DM"; standard= "DM"; canBeMid = false; caseValue = StreetType.Dm }
            { primary = "DIVIDE"; common = "DIV"; standard= "DV"; canBeMid = false; caseValue = StreetType.Dv }
            { primary = "DIVIDE"; common = "DIVIDE"; standard= "DV"; canBeMid = false; caseValue = StreetType.Dv }
            { primary = "DIVIDE"; common = "DV"; standard= "DV"; canBeMid = false; caseValue = StreetType.Dv }
            { primary = "DIVIDE"; common = "DVD"; standard= "DV"; canBeMid = false; caseValue = StreetType.Dv }
            { primary = "DRIVE"; common = "DR"; standard= "DR"; canBeMid = false; caseValue = StreetType.Dr }
            { primary = "DRIVE"; common = "DRIV"; standard= "DR"; canBeMid = false; caseValue = StreetType.Dr }
            { primary = "DRIVE"; common = "DRIVE"; standard= "DR"; canBeMid = false; caseValue = StreetType.Dr }
            { primary = "DRIVE"; common = "DRV"; standard= "DR"; canBeMid = false; caseValue = StreetType.Dr }
            { primary = "DRIVES"; common = "DRIVES"; standard= "DRS"; canBeMid = false; caseValue = StreetType.Drs }

            // Weird street type about which even post office does not know.
            { primary = "END"; common = "END"; standard= "END"; canBeMid = false; caseValue = StreetType.End }

            { primary = "ESTATE"; common = "EST"; standard= "EST"; canBeMid = false; caseValue = StreetType.Est }
            { primary = "ESTATE"; common = "ESTATE"; standard= "EST"; canBeMid = false; caseValue = StreetType.Est }
            { primary = "ESTATES"; common = "ESTATES"; standard= "ESTS"; canBeMid = false; caseValue = StreetType.Ests }
            { primary = "ESTATES"; common = "ESTS"; standard= "ESTS"; canBeMid = false; caseValue = StreetType.Ests }
            { primary = "EXPRESSWAY"; common = "EXP"; standard= "EXPY"; canBeMid = false; caseValue = StreetType.Expy }
            { primary = "EXPRESSWAY"; common = "EXPR"; standard= "EXPY"; canBeMid = false; caseValue = StreetType.Expy }
            { primary = "EXPRESSWAY"; common = "EXPRESS"; standard= "EXPY"; canBeMid = false; caseValue = StreetType.Expy }
            { primary = "EXPRESSWAY"; common = "EXPRESSWAY"; standard= "EXPY"; canBeMid = false; caseValue = StreetType.Expy }
            { primary = "EXPRESSWAY"; common = "EXPW"; standard= "EXPY"; canBeMid = false; caseValue = StreetType.Expy }
            { primary = "EXPRESSWAY"; common = "EXPY"; standard= "EXPY"; canBeMid = false; caseValue = StreetType.Expy }
            { primary = "EXTENSION"; common = "EXT"; standard= "EXT"; canBeMid = false; caseValue = StreetType.Ext }
            { primary = "EXTENSION"; common = "EXTENSION"; standard= "EXT"; canBeMid = false; caseValue = StreetType.Ext }
            { primary = "EXTENSION"; common = "EXTN"; standard= "EXT"; canBeMid = false; caseValue = StreetType.Ext }
            { primary = "EXTENSION"; common = "EXTNSN"; standard= "EXT"; canBeMid = false; caseValue = StreetType.Ext }
            { primary = "EXTENSIONS"; common = "EXTS"; standard= "EXTS"; canBeMid = false; caseValue = StreetType.Exts }
            { primary = "FALL"; common = "FALL"; standard= "FALL"; canBeMid = false; caseValue = StreetType.Fall }
            { primary = "FALLS"; common = "FALLS"; standard= "FLS"; canBeMid = false; caseValue = StreetType.Fls }
            { primary = "FALLS"; common = "FLS"; standard= "FLS"; canBeMid = false; caseValue = StreetType.Fls }
            { primary = "FERRY"; common = "FERRY"; standard= "FRY"; canBeMid = false; caseValue = StreetType.Fry }
            { primary = "FERRY"; common = "FRRY"; standard= "FRY"; canBeMid = false; caseValue = StreetType.Fry }
            { primary = "FERRY"; common = "FRY"; standard= "FRY"; canBeMid = false; caseValue = StreetType.Fry }
            { primary = "FIELD"; common = "FIELD"; standard= "FLD"; canBeMid = false; caseValue = StreetType.Fld }
            { primary = "FIELD"; common = "FLD"; standard= "FLD"; canBeMid = false; caseValue = StreetType.Fld }
            { primary = "FIELDS"; common = "FIELDS"; standard= "FLDS"; canBeMid = false; caseValue = StreetType.Flds }
            { primary = "FIELDS"; common = "FLDS"; standard= "FLDS"; canBeMid = false; caseValue = StreetType.Flds }
            { primary = "FLAT"; common = "FLAT"; standard= "FLT"; canBeMid = false; caseValue = StreetType.Flt }
            { primary = "FLAT"; common = "FLT"; standard= "FLT"; canBeMid = false; caseValue = StreetType.Flt }
            { primary = "FLATS"; common = "FLATS"; standard= "FLTS"; canBeMid = false; caseValue = StreetType.Flts }
            { primary = "FLATS"; common = "FLTS"; standard= "FLTS"; canBeMid = false; caseValue = StreetType.Flts }
            { primary = "FORD"; common = "FORD"; standard= "FRD"; canBeMid = false; caseValue = StreetType.Frd }
            { primary = "FORD"; common = "FRD"; standard= "FRD"; canBeMid = false; caseValue = StreetType.Frd }
            { primary = "FORDS"; common = "FORDS"; standard= "FRDS"; canBeMid = false; caseValue = StreetType.Frds }
            { primary = "FOREST"; common = "FOREST"; standard= "FRST"; canBeMid = false; caseValue = StreetType.Frst }
            { primary = "FOREST"; common = "FORESTS"; standard= "FRST"; canBeMid = false; caseValue = StreetType.Frst }
            { primary = "FOREST"; common = "FRST"; standard= "FRST"; canBeMid = false; caseValue = StreetType.Frst }
            { primary = "FORGE"; common = "FORG"; standard= "FRG"; canBeMid = false; caseValue = StreetType.Frg }
            { primary = "FORGE"; common = "FORGE"; standard= "FRG"; canBeMid = false; caseValue = StreetType.Frg }
            { primary = "FORGE"; common = "FRG"; standard= "FRG"; canBeMid = false; caseValue = StreetType.Frg }
            { primary = "FORGES"; common = "FORGES"; standard= "FRGS"; canBeMid = false; caseValue = StreetType.Frgs }
            { primary = "FORK"; common = "FORK"; standard= "FRK"; canBeMid = false; caseValue = StreetType.Frk }
            { primary = "FORK"; common = "FRK"; standard= "FRK"; canBeMid = false; caseValue = StreetType.Frk }
            { primary = "FORKS"; common = "FORKS"; standard= "FRKS"; canBeMid = false; caseValue = StreetType.Frks }
            { primary = "FORKS"; common = "FRKS"; standard= "FRKS"; canBeMid = false; caseValue = StreetType.Frks }
            { primary = "FORT"; common = "FORT"; standard= "FT"; canBeMid = false; caseValue = StreetType.Ft }
            { primary = "FORT"; common = "FRT"; standard= "FT"; canBeMid = false; caseValue = StreetType.Ft }
            { primary = "FORT"; common = "FT"; standard= "FT"; canBeMid = false; caseValue = StreetType.Ft }
            { primary = "FREEWAY"; common = "FREEWAY"; standard= "FWY"; canBeMid = false; caseValue = StreetType.Fwy }
            { primary = "FREEWAY"; common = "FREEWY"; standard= "FWY"; canBeMid = false; caseValue = StreetType.Fwy }
            { primary = "FREEWAY"; common = "FRWAY"; standard= "FWY"; canBeMid = false; caseValue = StreetType.Fwy }
            { primary = "FREEWAY"; common = "FRWY"; standard= "FWY"; canBeMid = false; caseValue = StreetType.Fwy }
            { primary = "FREEWAY"; common = "FWY"; standard= "FWY"; canBeMid = false; caseValue = StreetType.Fwy }
            { primary = "GARDEN"; common = "GARDEN"; standard= "GDN"; canBeMid = false; caseValue = StreetType.Gdn }
            { primary = "GARDEN"; common = "GARDN"; standard= "GDN"; canBeMid = false; caseValue = StreetType.Gdn }
            { primary = "GARDEN"; common = "GRDEN"; standard= "GDN"; canBeMid = false; caseValue = StreetType.Gdn }
            { primary = "GARDEN"; common = "GRDN"; standard= "GDN"; canBeMid = false; caseValue = StreetType.Gdn }
            { primary = "GARDENS"; common = "GARDENS"; standard= "GDNS"; canBeMid = false; caseValue = StreetType.Gdns }
            { primary = "GARDENS"; common = "GDNS"; standard= "GDNS"; canBeMid = false; caseValue = StreetType.Gdns }
            { primary = "GARDENS"; common = "GRDNS"; standard= "GDNS"; canBeMid = false; caseValue = StreetType.Gdns }
            { primary = "GATEWAY"; common = "GATEWAY"; standard= "GTWY"; canBeMid = false; caseValue = StreetType.Gtwy }
            { primary = "GATEWAY"; common = "GATEWY"; standard= "GTWY"; canBeMid = false; caseValue = StreetType.Gtwy }
            { primary = "GATEWAY"; common = "GATWAY"; standard= "GTWY"; canBeMid = false; caseValue = StreetType.Gtwy }
            { primary = "GATEWAY"; common = "GTWAY"; standard= "GTWY"; canBeMid = false; caseValue = StreetType.Gtwy }
            { primary = "GATEWAY"; common = "GTWY"; standard= "GTWY"; canBeMid = false; caseValue = StreetType.Gtwy }
            { primary = "GLEN"; common = "GLEN"; standard= "GLN"; canBeMid = false; caseValue = StreetType.Gln }
            { primary = "GLEN"; common = "GLN"; standard= "GLN"; canBeMid = false; caseValue = StreetType.Gln }
            { primary = "GLENS"; common = "GLENS"; standard= "GLNS"; canBeMid = false; caseValue = StreetType.Glns }
            { primary = "GREEN"; common = "GREEN"; standard= "GRN"; canBeMid = false; caseValue = StreetType.Grn }
            { primary = "GREEN"; common = "GRN"; standard= "GRN"; canBeMid = false; caseValue = StreetType.Grn }
            { primary = "GREENS"; common = "GREENS"; standard= "GRNS"; canBeMid = false; caseValue = StreetType.Grns }
            { primary = "GROVE"; common = "GROV"; standard= "GRV"; canBeMid = false; caseValue = StreetType.Grv }
            { primary = "GROVE"; common = "GROVE"; standard= "GRV"; canBeMid = false; caseValue = StreetType.Grv }
            { primary = "GROVE"; common = "GRV"; standard= "GRV"; canBeMid = false; caseValue = StreetType.Grv }
            { primary = "GROVES"; common = "GROVES"; standard= "GRVS"; canBeMid = false; caseValue = StreetType.Grvs }
            { primary = "HARBOR"; common = "HARB"; standard= "HBR"; canBeMid = false; caseValue = StreetType.Hbr }
            { primary = "HARBOR"; common = "HARBOR"; standard= "HBR"; canBeMid = false; caseValue = StreetType.Hbr }
            { primary = "HARBOR"; common = "HARBR"; standard= "HBR"; canBeMid = false; caseValue = StreetType.Hbr }
            { primary = "HARBOR"; common = "HBR"; standard= "HBR"; canBeMid = false; caseValue = StreetType.Hbr }
            { primary = "HARBOR"; common = "HRBOR"; standard= "HBR"; canBeMid = false; caseValue = StreetType.Hbr }
            { primary = "HARBORS"; common = "HARBORS"; standard= "HBRS"; canBeMid = false; caseValue = StreetType.Hbrs }
            { primary = "HAVEN"; common = "HAVEN"; standard= "HVN"; canBeMid = false; caseValue = StreetType.Hvn }
            { primary = "HAVEN"; common = "HVN"; standard= "HVN"; canBeMid = false; caseValue = StreetType.Hvn }
            { primary = "HEIGHTS"; common = "HT"; standard= "HTS"; canBeMid = false; caseValue = StreetType.Hts }
            { primary = "HEIGHTS"; common = "HTS"; standard= "HTS"; canBeMid = false; caseValue = StreetType.Hts }
            { primary = "HIGHWAY"; common = "HIGHWAY"; standard= "HWY"; canBeMid = true; caseValue = StreetType.Hwy }

            // Glitch in MD
            { primary = "HIGHWAY"; common = "HIGHWA"; standard= "HWY"; canBeMid = true; caseValue = StreetType.Hwy }

            { primary = "HIGHWAY"; common = "HIGHWY"; standard= "HWY"; canBeMid = true; caseValue = StreetType.Hwy }
            { primary = "HIGHWAY"; common = "HIWAY"; standard= "HWY"; canBeMid = true; caseValue = StreetType.Hwy }
            { primary = "HIGHWAY"; common = "HIWY"; standard= "HWY"; canBeMid = true; caseValue = StreetType.Hwy }
            { primary = "HIGHWAY"; common = "HWAY"; standard= "HWY"; canBeMid = true; caseValue = StreetType.Hwy }
            { primary = "HIGHWAY"; common = "HWY"; standard= "HWY"; canBeMid = true; caseValue = StreetType.Hwy }
            { primary = "HILL"; common = "HILL"; standard= "HL"; canBeMid = false; caseValue = StreetType.Hl }
            { primary = "HILL"; common = "HL"; standard= "HL"; canBeMid = false; caseValue = StreetType.Hl }
            { primary = "HILLS"; common = "HILLS"; standard= "HLS"; canBeMid = false; caseValue = StreetType.Hls }
            { primary = "HILLS"; common = "HLS"; standard= "HLS"; canBeMid = false; caseValue = StreetType.Hls }
            { primary = "HOLLOW"; common = "HLLW"; standard= "HOLW"; canBeMid = false; caseValue = StreetType.Holw }
            { primary = "HOLLOW"; common = "HOLLOW"; standard= "HOLW"; canBeMid = false; caseValue = StreetType.Holw }
            { primary = "HOLLOW"; common = "HOLLOWS"; standard= "HOLW"; canBeMid = false; caseValue = StreetType.Holw }
            { primary = "HOLLOW"; common = "HOLW"; standard= "HOLW"; canBeMid = false; caseValue = StreetType.Holw }
            { primary = "HOLLOW"; common = "HOLWS"; standard= "HOLW"; canBeMid = false; caseValue = StreetType.Holw }
            { primary = "INLET"; common = "INLT"; standard= "INLT"; canBeMid = false; caseValue = StreetType.Inlt }
            { primary = "ISLAND"; common = "IS"; standard= "IS"; canBeMid = false; caseValue = StreetType.Is }
            { primary = "ISLAND"; common = "ISLAND"; standard= "IS"; canBeMid = false; caseValue = StreetType.Is }
            { primary = "ISLAND"; common = "ISLND"; standard= "IS"; canBeMid = false; caseValue = StreetType.Is }
            { primary = "ISLANDS"; common = "ISLANDS"; standard= "ISS"; canBeMid = false; caseValue = StreetType.Iss }
            { primary = "ISLANDS"; common = "ISLNDS"; standard= "ISS"; canBeMid = false; caseValue = StreetType.Iss }
            { primary = "ISLANDS"; common = "ISS"; standard= "ISS"; canBeMid = false; caseValue = StreetType.Iss }
            { primary = "ISLE"; common = "ISLE"; standard= "ISLE"; canBeMid = false; caseValue = StreetType.Isle }
            { primary = "ISLE"; common = "ISLES"; standard= "ISLE"; canBeMid = false; caseValue = StreetType.Isle }
            { primary = "JUNCTION"; common = "JCT"; standard= "JCT"; canBeMid = false; caseValue = StreetType.Jct }
            { primary = "JUNCTION"; common = "JCTION"; standard= "JCT"; canBeMid = false; caseValue = StreetType.Jct }
            { primary = "JUNCTION"; common = "JCTN"; standard= "JCT"; canBeMid = false; caseValue = StreetType.Jct }
            { primary = "JUNCTION"; common = "JUNCTION"; standard= "JCT"; canBeMid = false; caseValue = StreetType.Jct }
            { primary = "JUNCTION"; common = "JUNCTN"; standard= "JCT"; canBeMid = false; caseValue = StreetType.Jct }
            { primary = "JUNCTION"; common = "JUNCTON"; standard= "JCT"; canBeMid = false; caseValue = StreetType.Jct }
            { primary = "JUNCTIONS"; common = "JCTNS"; standard= "JCTS"; canBeMid = false; caseValue = StreetType.Jcts }
            { primary = "JUNCTIONS"; common = "JCTS"; standard= "JCTS"; canBeMid = false; caseValue = StreetType.Jcts }
            { primary = "JUNCTIONS"; common = "JUNCTIONS"; standard= "JCTS"; canBeMid = false; caseValue = StreetType.Jcts }
            { primary = "KEY"; common = "KEY"; standard= "KY"; canBeMid = false; caseValue = StreetType.Ky }
            { primary = "KEY"; common = "KY"; standard= "KY"; canBeMid = false; caseValue = StreetType.Ky }
            { primary = "KEYS"; common = "KEYS"; standard= "KYS"; canBeMid = false; caseValue = StreetType.Kys }
            { primary = "KEYS"; common = "KYS"; standard= "KYS"; canBeMid = false; caseValue = StreetType.Kys }
            { primary = "KNOLL"; common = "KNL"; standard= "KNL"; canBeMid = false; caseValue = StreetType.Knl }
            { primary = "KNOLL"; common = "KNOL"; standard= "KNL"; canBeMid = false; caseValue = StreetType.Knl }
            { primary = "KNOLL"; common = "KNOLL"; standard= "KNL"; canBeMid = false; caseValue = StreetType.Knl }
            { primary = "KNOLLS"; common = "KNLS"; standard= "KNLS"; canBeMid = false; caseValue = StreetType.Knls }
            { primary = "KNOLLS"; common = "KNOLLS"; standard= "KNLS"; canBeMid = false; caseValue = StreetType.Knls }
            { primary = "LAKE"; common = "LK"; standard= "LK"; canBeMid = false; caseValue = StreetType.Lk }
            { primary = "LAKE"; common = "LAKE"; standard= "LK"; canBeMid = false; caseValue = StreetType.Lk }
            { primary = "LAKES"; common = "LKS"; standard= "LKS"; canBeMid = false; caseValue = StreetType.Lks }
            { primary = "LAKES"; common = "LAKES"; standard= "LKS"; canBeMid = false; caseValue = StreetType.Lks }
            { primary = "LAND"; common = "LAND"; standard= "LAND"; canBeMid = false; caseValue = StreetType.Land }
            { primary = "LANDING"; common = "LANDING"; standard= "LNDG"; canBeMid = false; caseValue = StreetType.Lndg }
            { primary = "LANDING"; common = "LNDG"; standard= "LNDG"; canBeMid = false; caseValue = StreetType.Lndg }
            { primary = "LANDING"; common = "LNDNG"; standard= "LNDG"; canBeMid = false; caseValue = StreetType.Lndg }
            { primary = "LANE"; common = "LANE"; standard= "LN"; canBeMid = false; caseValue = StreetType.Ln }
            { primary = "LANE"; common = "LN"; standard= "LN"; canBeMid = false; caseValue = StreetType.Ln }
            { primary = "LIGHT"; common = "LGT"; standard= "LGT"; canBeMid = false; caseValue = StreetType.Lgt }
            { primary = "LIGHT"; common = "LIGHT"; standard= "LGT"; canBeMid = false; caseValue = StreetType.Lgt }
            { primary = "LIGHTS"; common = "LIGHTS"; standard= "LGTS"; canBeMid = false; caseValue = StreetType.Lgts }
            { primary = "LOAF"; common = "LF"; standard= "LF"; canBeMid = false; caseValue = StreetType.Lf }
            { primary = "LOAF"; common = "LOAF"; standard= "LF"; canBeMid = false; caseValue = StreetType.Lf }
            { primary = "LOCK"; common = "LCK"; standard= "LCK"; canBeMid = false; caseValue = StreetType.Lck }
            { primary = "LOCK"; common = "LOCK"; standard= "LCK"; canBeMid = false; caseValue = StreetType.Lck }
            { primary = "LOCKS"; common = "LCKS"; standard= "LCKS"; canBeMid = false; caseValue = StreetType.Lcks }
            { primary = "LOCKS"; common = "LOCKS"; standard= "LCKS"; canBeMid = false; caseValue = StreetType.Lcks }
            { primary = "LODGE"; common = "LDG"; standard= "LDG"; canBeMid = false; caseValue = StreetType.Ldg }
            { primary = "LODGE"; common = "LDGE"; standard= "LDG"; canBeMid = false; caseValue = StreetType.Ldg }
            { primary = "LODGE"; common = "LODG"; standard= "LDG"; canBeMid = false; caseValue = StreetType.Ldg }
            { primary = "LODGE"; common = "LODGE"; standard= "LDG"; canBeMid = false; caseValue = StreetType.Ldg }
            { primary = "LOOP"; common = "LOOP"; standard= "LOOP"; canBeMid = false; caseValue = StreetType.Loop }
            { primary = "LOOP"; common = "LOOPS"; standard= "LOOP"; canBeMid = false; caseValue = StreetType.Loop }
            { primary = "MALL"; common = "MALL"; standard= "MALL"; canBeMid = false; caseValue = StreetType.Mall }
            { primary = "MANOR"; common = "MNR"; standard= "MNR"; canBeMid = false; caseValue = StreetType.Mnr }
            { primary = "MANOR"; common = "MANOR"; standard= "MNR"; canBeMid = false; caseValue = StreetType.Mnr }
            { primary = "MANORS"; common = "MANORS"; standard= "MNRS"; canBeMid = false; caseValue = StreetType.Mnrs }
            { primary = "MANORS"; common = "MNRS"; standard= "MNRS"; canBeMid = false; caseValue = StreetType.Mnrs }
            { primary = "MEADOW"; common = "MEADOW"; standard= "MDW"; canBeMid = false; caseValue = StreetType.Mdw }
            { primary = "MEADOWS"; common = "MDW"; standard= "MDWS"; canBeMid = false; caseValue = StreetType.Mdws }
            { primary = "MEADOWS"; common = "MDWS"; standard= "MDWS"; canBeMid = false; caseValue = StreetType.Mdws }
            { primary = "MEADOWS"; common = "MEADOWS"; standard= "MDWS"; canBeMid = false; caseValue = StreetType.Mdws }
            { primary = "MEADOWS"; common = "MEDOWS"; standard= "MDWS"; canBeMid = false; caseValue = StreetType.Mdws }
            { primary = "MEWS"; common = "MEWS"; standard= "MEWS"; canBeMid = false; caseValue = StreetType.Mews }
            { primary = "MILL"; common = "MILL"; standard= "ML"; canBeMid = false; caseValue = StreetType.Ml }
            { primary = "MILLS"; common = "MILLS"; standard= "MLS"; canBeMid = false; caseValue = StreetType.Mls }
            { primary = "MISSION"; common = "MISSN"; standard= "MSN"; canBeMid = false; caseValue = StreetType.Msn }
            { primary = "MISSION"; common = "MSSN"; standard= "MSN"; canBeMid = false; caseValue = StreetType.Msn }
            { primary = "MOTORWAY"; common = "MOTORWAY"; standard= "MTWY"; canBeMid = false; caseValue = StreetType.Mtwy }
            { primary = "MOUNT"; common = "MNT"; standard= "MT"; canBeMid = false; caseValue = StreetType.Mt }
            { primary = "MOUNT"; common = "MT"; standard= "MT"; canBeMid = false; caseValue = StreetType.Mt }
            { primary = "MOUNT"; common = "MOUNT"; standard= "MT"; canBeMid = false; caseValue = StreetType.Mt }
            { primary = "MOUNTAIN"; common = "MNTAIN"; standard= "MTN"; canBeMid = false; caseValue = StreetType.Mtn }
            { primary = "MOUNTAIN"; common = "MNTN"; standard= "MTN"; canBeMid = false; caseValue = StreetType.Mtn }
            { primary = "MOUNTAIN"; common = "MOUNTAIN"; standard= "MTN"; canBeMid = false; caseValue = StreetType.Mtn }
            { primary = "MOUNTAIN"; common = "MOUNTIN"; standard= "MTN"; canBeMid = false; caseValue = StreetType.Mtn }
            { primary = "MOUNTAIN"; common = "MTIN"; standard= "MTN"; canBeMid = false; caseValue = StreetType.Mtn }
            { primary = "MOUNTAIN"; common = "MTN"; standard= "MTN"; canBeMid = false; caseValue = StreetType.Mtn }
            { primary = "MOUNTAINS"; common = "MNTNS"; standard= "MTNS"; canBeMid = false; caseValue = StreetType.Mtns }
            { primary = "MOUNTAINS"; common = "MOUNTAINS"; standard= "MTNS"; canBeMid = false; caseValue = StreetType.Mtns }
            { primary = "NECK"; common = "NCK"; standard= "NCK"; canBeMid = false; caseValue = StreetType.Nck }
            { primary = "NECK"; common = "NECK"; standard= "NCK"; canBeMid = false; caseValue = StreetType.Nck }
            { primary = "ORCHARD"; common = "ORCH"; standard= "ORCH"; canBeMid = false; caseValue = StreetType.Orch }
            { primary = "ORCHARD"; common = "ORCHARD"; standard= "ORCH"; canBeMid = false; caseValue = StreetType.Orch }
            { primary = "ORCHARD"; common = "ORCHRD"; standard= "ORCH"; canBeMid = false; caseValue = StreetType.Orch }
            { primary = "OVAL"; common = "OVAL"; standard= "OVAL"; canBeMid = false; caseValue = StreetType.Oval }
            { primary = "OVAL"; common = "OVL"; standard= "OVAL"; canBeMid = false; caseValue = StreetType.Oval }
            { primary = "OVERPASS"; common = "OVERPASS"; standard= "OPAS"; canBeMid = false; caseValue = StreetType.Opas }
            { primary = "PARK"; common = "PARK"; standard= "PARK"; canBeMid = false; caseValue = StreetType.Park }
            { primary = "PARK"; common = "PRK"; standard= "PARK"; canBeMid = false; caseValue = StreetType.Park }
            { primary = "PARKS"; common = "PARKS"; standard= "PARK"; canBeMid = false; caseValue = StreetType.Park }
            { primary = "PARKWAY"; common = "PARKWAY"; standard= "PKWY"; canBeMid = false; caseValue = StreetType.Pkwy }
            { primary = "PARKWAY"; common = "PARKWY"; standard= "PKWY"; canBeMid = false; caseValue = StreetType.Pkwy }
            { primary = "PARKWAY"; common = "PKWAY"; standard= "PKWY"; canBeMid = false; caseValue = StreetType.Pkwy }
            { primary = "PARKWAY"; common = "PKWY"; standard= "PKWY"; canBeMid = false; caseValue = StreetType.Pkwy }
            { primary = "PARKWAY"; common = "PKY"; standard= "PKWY"; canBeMid = false; caseValue = StreetType.Pkwy }
            { primary = "PARKWAYS"; common = "PARKWAYS"; standard= "PKWY"; canBeMid = false; caseValue = StreetType.Pkwy }
            { primary = "PARKWAYS"; common = "PKWYS"; standard= "PKWY"; canBeMid = false; caseValue = StreetType.Pkwy }
            { primary = "PASS"; common = "PASS"; standard= "PASS"; canBeMid = false; caseValue = StreetType.Pass }
            { primary = "PASSAGE"; common = "PASSAGE"; standard= "PSGE"; canBeMid = false; caseValue = StreetType.Psge }
            { primary = "PATH"; common = "PATH"; standard= "PATH"; canBeMid = false; caseValue = StreetType.Path }
            { primary = "PATH"; common = "PATHS"; standard= "PATH"; canBeMid = false; caseValue = StreetType.Path }
            { primary = "PIKE"; common = "PIKE"; standard= "PIKE"; canBeMid = false; caseValue = StreetType.Pike }
            { primary = "PIKE"; common = "PIKES"; standard= "PIKE"; canBeMid = false; caseValue = StreetType.Pike }
            { primary = "PINE"; common = "PINE"; standard= "PNE"; canBeMid = false; caseValue = StreetType.Pne }
            { primary = "PINES"; common = "PINES"; standard= "PNES"; canBeMid = false; caseValue = StreetType.Pnes }
            { primary = "PINES"; common = "PNES"; standard= "PNES"; canBeMid = false; caseValue = StreetType.Pnes }
            { primary = "PLACE"; common = "PL"; standard= "PL"; canBeMid = false; caseValue = StreetType.Pl }
            { primary = "PLAIN"; common = "PLAIN"; standard= "PLN"; canBeMid = false; caseValue = StreetType.Pln }
            { primary = "PLAIN"; common = "PLN"; standard= "PLN"; canBeMid = false; caseValue = StreetType.Pln }
            { primary = "PLAINS"; common = "PLAINS"; standard= "PLNS"; canBeMid = false; caseValue = StreetType.Plns }
            { primary = "PLAINS"; common = "PLNS"; standard= "PLNS"; canBeMid = false; caseValue = StreetType.Plns }
            { primary = "PLAZA"; common = "PLAZA"; standard= "PLZ"; canBeMid = false; caseValue = StreetType.Plz }
            { primary = "PLAZA"; common = "PLZ"; standard= "PLZ"; canBeMid = false; caseValue = StreetType.Plz }
            { primary = "PLAZA"; common = "PLZA"; standard= "PLZ"; canBeMid = false; caseValue = StreetType.Plz }
            { primary = "POINT"; common = "POINT"; standard= "PT"; canBeMid = false; caseValue = StreetType.Pt }

            // Some weird spelling of POINT, like in "4673 Andrea Pointe, Marietta, GA"
            { primary = "POINTE"; common = "POINTE"; standard= "PT"; canBeMid = false; caseValue = StreetType.Pt }

            { primary = "POINT"; common = "PT"; standard= "PT"; canBeMid = false; caseValue = StreetType.Pt }
            { primary = "POINTS"; common = "POINTS"; standard= "PTS"; canBeMid = false; caseValue = StreetType.Pts }
            { primary = "POINTS"; common = "PTS"; standard= "PTS"; canBeMid = false; caseValue = StreetType.Pts }
            { primary = "PORT"; common = "PORT"; standard= "PRT"; canBeMid = false; caseValue = StreetType.Prt }
            { primary = "PORT"; common = "PRT"; standard= "PRT"; canBeMid = false; caseValue = StreetType.Prt }
            { primary = "PORTS"; common = "PORTS"; standard= "PRTS"; canBeMid = false; caseValue = StreetType.Prts }
            { primary = "PORTS"; common = "PRTS"; standard= "PRTS"; canBeMid = false; caseValue = StreetType.Prts }
            { primary = "PRAIRIE"; common = "PR"; standard= "PR"; canBeMid = false; caseValue = StreetType.Pr }
            { primary = "PRAIRIE"; common = "PRAIRIE"; standard= "PR"; canBeMid = false; caseValue = StreetType.Pr }
            { primary = "PRAIRIE"; common = "PRR"; standard= "PR"; canBeMid = false; caseValue = StreetType.Pr }
            { primary = "RADIAL"; common = "RAD"; standard= "RADL"; canBeMid = false; caseValue = StreetType.Radl }
            { primary = "RADIAL"; common = "RADIAL"; standard= "RADL"; canBeMid = false; caseValue = StreetType.Radl }
            { primary = "RADIAL"; common = "RADIEL"; standard= "RADL"; canBeMid = false; caseValue = StreetType.Radl }
            { primary = "RADIAL"; common = "RADL"; standard= "RADL"; canBeMid = false; caseValue = StreetType.Radl }
            { primary = "RAMP"; common = "RAMP"; standard= "RAMP"; canBeMid = false; caseValue = StreetType.Ramp }
            { primary = "RANCH"; common = "RANCH"; standard= "RNCH"; canBeMid = false; caseValue = StreetType.Rnch }
            { primary = "RANCH"; common = "RANCHES"; standard= "RNCH"; canBeMid = false; caseValue = StreetType.Rnch }
            { primary = "RANCH"; common = "RNCH"; standard= "RNCH"; canBeMid = false; caseValue = StreetType.Rnch }
            { primary = "RANCH"; common = "RNCHS"; standard= "RNCH"; canBeMid = false; caseValue = StreetType.Rnch }
            { primary = "RAPID"; common = "RAPID"; standard= "RPD"; canBeMid = false; caseValue = StreetType.Rpd }
            { primary = "RAPID"; common = "RPD"; standard= "RPD"; canBeMid = false; caseValue = StreetType.Rpd }
            { primary = "RAPIDS"; common = "RAPIDS"; standard= "RPDS"; canBeMid = false; caseValue = StreetType.Rpds }
            { primary = "RAPIDS"; common = "RPDS"; standard= "RPDS"; canBeMid = false; caseValue = StreetType.Rpds }
            { primary = "REST"; common = "REST"; standard= "RST"; canBeMid = false; caseValue = StreetType.Rst }
            { primary = "REST"; common = "RST"; standard= "RST"; canBeMid = false; caseValue = StreetType.Rst }
            { primary = "RIDGE"; common = "RDG"; standard= "RDG"; canBeMid = false; caseValue = StreetType.Rdg }
            { primary = "RIDGE"; common = "RDGE"; standard= "RDG"; canBeMid = false; caseValue = StreetType.Rdg }
            { primary = "RIDGE"; common = "RIDGE"; standard= "RDG"; canBeMid = false; caseValue = StreetType.Rdg }
            { primary = "RIDGES"; common = "RDGS"; standard= "RDGS"; canBeMid = false; caseValue = StreetType.Rdgs }
            { primary = "RIDGES"; common = "RIDGES"; standard= "RDGS"; canBeMid = false; caseValue = StreetType.Rdgs }
            { primary = "RIVER"; common = "RIV"; standard= "RIV"; canBeMid = false; caseValue = StreetType.Riv }
            { primary = "RIVER"; common = "RIVER"; standard= "RIV"; canBeMid = false; caseValue = StreetType.Riv }
            { primary = "RIVER"; common = "RVR"; standard= "RIV"; canBeMid = false; caseValue = StreetType.Riv }
            { primary = "RIVER"; common = "RIVR"; standard= "RIV"; canBeMid = false; caseValue = StreetType.Riv }
            { primary = "ROAD"; common = "RD"; standard= "RD"; canBeMid = true; caseValue = StreetType.Rd }
            { primary = "ROAD"; common = "ROAD"; standard= "RD"; canBeMid = true; caseValue = StreetType.Rd }
            { primary = "ROADS"; common = "ROADS"; standard= "RDS"; canBeMid = false; caseValue = StreetType.Rds }
            { primary = "ROADS"; common = "RDS"; standard= "RDS"; canBeMid = false; caseValue = StreetType.Rds }
            { primary = "ROUTE"; common = "ROUTE"; standard= "RTE"; canBeMid = true; caseValue = StreetType.Rte }
            { primary = "ROW"; common = "ROW"; standard= "ROW"; canBeMid = false; caseValue = StreetType.Row }
            { primary = "RUE"; common = "RUE"; standard= "RUE"; canBeMid = false; caseValue = StreetType.Rue }
            { primary = "RUN"; common = "RUN"; standard= "RUN"; canBeMid = false; caseValue = StreetType.Run }
            { primary = "SHOAL"; common = "SHL"; standard= "SHL"; canBeMid = false; caseValue = StreetType.Shl }
            { primary = "SHOAL"; common = "SHOAL"; standard= "SHL"; canBeMid = false; caseValue = StreetType.Shl }
            { primary = "SHOALS"; common = "SHLS"; standard= "SHLS"; canBeMid = false; caseValue = StreetType.Shls }
            { primary = "SHOALS"; common = "SHOALS"; standard= "SHLS"; canBeMid = false; caseValue = StreetType.Shls }
            { primary = "SHORE"; common = "SHOAR"; standard= "SHR"; canBeMid = false; caseValue = StreetType.Shr }
            { primary = "SHORE"; common = "SHORE"; standard= "SHR"; canBeMid = false; caseValue = StreetType.Shr }
            { primary = "SHORE"; common = "SHR"; standard= "SHR"; canBeMid = false; caseValue = StreetType.Shr }
            { primary = "SHORES"; common = "SHOARS"; standard= "SHRS"; canBeMid = false; caseValue = StreetType.Shrs }
            { primary = "SHORES"; common = "SHORES"; standard= "SHRS"; canBeMid = false; caseValue = StreetType.Shrs }
            { primary = "SHORES"; common = "SHRS"; standard= "SHRS"; canBeMid = false; caseValue = StreetType.Shrs }
            { primary = "SKYWAY"; common = "SKYWAY"; standard= "SKWY"; canBeMid = false; caseValue = StreetType.Skwy }
            { primary = "SPRING"; common = "SPG"; standard= "SPG"; canBeMid = false; caseValue = StreetType.Spg }
            { primary = "SPRING"; common = "SPNG"; standard= "SPG"; canBeMid = false; caseValue = StreetType.Spg }
            { primary = "SPRING"; common = "SPRING"; standard= "SPG"; canBeMid = false; caseValue = StreetType.Spg }
            { primary = "SPRING"; common = "SPRNG"; standard= "SPG"; canBeMid = false; caseValue = StreetType.Spg }
            { primary = "SPRINGS"; common = "SPGS"; standard= "SPGS"; canBeMid = false; caseValue = StreetType.Spgs }
            { primary = "SPRINGS"; common = "SPNGS"; standard= "SPGS"; canBeMid = false; caseValue = StreetType.Spgs }
            { primary = "SPRINGS"; common = "SPRINGS"; standard= "SPGS"; canBeMid = false; caseValue = StreetType.Spgs }
            { primary = "SPRINGS"; common = "SPRNGS"; standard= "SPGS"; canBeMid = false; caseValue = StreetType.Spgs }
            { primary = "SPUR"; common = "SPUR"; standard= "SPUR"; canBeMid = false; caseValue = StreetType.Spur }
            { primary = "SPURS"; common = "SPURS"; standard= "SPUR"; canBeMid = false; caseValue = StreetType.Spur }
            { primary = "SQUARE"; common = "SQ"; standard= "SQ"; canBeMid = false; caseValue = StreetType.Sq }
            { primary = "SQUARE"; common = "SQR"; standard= "SQ"; canBeMid = false; caseValue = StreetType.Sq }
            { primary = "SQUARE"; common = "SQRE"; standard= "SQ"; canBeMid = false; caseValue = StreetType.Sq }
            { primary = "SQUARE"; common = "SQU"; standard= "SQ"; canBeMid = false; caseValue = StreetType.Sq }
            { primary = "SQUARE"; common = "SQUARE"; standard= "SQ"; canBeMid = false; caseValue = StreetType.Sq }
            { primary = "SQUARES"; common = "SQRS"; standard= "SQS"; canBeMid = false; caseValue = StreetType.Sqs }
            { primary = "SQUARES"; common = "SQUARES"; standard= "SQS"; canBeMid = false; caseValue = StreetType.Sqs }
            { primary = "STATION"; common = "STA"; standard= "STA"; canBeMid = false; caseValue = StreetType.Sta }
            { primary = "STATION"; common = "STATION"; standard= "STA"; canBeMid = false; caseValue = StreetType.Sta }
            { primary = "STATION"; common = "STATN"; standard= "STA"; canBeMid = false; caseValue = StreetType.Sta }
            { primary = "STATION"; common = "STN"; standard= "STA"; canBeMid = false; caseValue = StreetType.Sta }
            { primary = "STRAVENUE"; common = "STRA"; standard= "STRA"; canBeMid = false; caseValue = StreetType.Stra }
            { primary = "STRAVENUE"; common = "STRAV"; standard= "STRA"; canBeMid = false; caseValue = StreetType.Stra }
            { primary = "STRAVENUE"; common = "STRAVEN"; standard= "STRA"; canBeMid = false; caseValue = StreetType.Stra }
            { primary = "STRAVENUE"; common = "STRAVENUE"; standard= "STRA"; canBeMid = false; caseValue = StreetType.Stra }
            { primary = "STRAVENUE"; common = "STRAVN"; standard= "STRA"; canBeMid = false; caseValue = StreetType.Stra }
            { primary = "STRAVENUE"; common = "STRVN"; standard= "STRA"; canBeMid = false; caseValue = StreetType.Stra }
            { primary = "STRAVENUE"; common = "STRVNUE"; standard= "STRA"; canBeMid = false; caseValue = StreetType.Stra }
            { primary = "STREAM"; common = "STREAM"; standard= "STRM"; canBeMid = false; caseValue = StreetType.Strm }
            { primary = "STREAM"; common = "STREME"; standard= "STRM"; canBeMid = false; caseValue = StreetType.Strm }
            { primary = "STREAM"; common = "STRM"; standard= "STRM"; canBeMid = false; caseValue = StreetType.Strm }
            { primary = "STREET"; common = "STREET"; standard= "ST"; canBeMid = false; caseValue = StreetType.St }
            { primary = "STREET"; common = "STRT"; standard= "ST"; canBeMid = false; caseValue = StreetType.St }
            { primary = "STREET"; common = "ST"; standard= "ST"; canBeMid = false; caseValue = StreetType.St }
            { primary = "STREET"; common = "STR"; standard= "ST"; canBeMid = false; caseValue = StreetType.St }
            { primary = "STREETS"; common = "STREETS"; standard= "STS"; canBeMid = false; caseValue = StreetType.Sts }
            { primary = "SUMMIT"; common = "SMT"; standard= "SMT"; canBeMid = false; caseValue = StreetType.Smt }
            { primary = "SUMMIT"; common = "SUMIT"; standard= "SMT"; canBeMid = false; caseValue = StreetType.Smt }
            { primary = "SUMMIT"; common = "SUMITT"; standard= "SMT"; canBeMid = false; caseValue = StreetType.Smt }
            { primary = "SUMMIT"; common = "SUMMIT"; standard= "SMT"; canBeMid = false; caseValue = StreetType.Smt }
            { primary = "TERRACE"; common = "TER"; standard= "TER"; canBeMid = false; caseValue = StreetType.Ter }
            { primary = "TERRACE"; common = "TERR"; standard= "TER"; canBeMid = false; caseValue = StreetType.Ter }
            { primary = "TERRACE"; common = "TERRACE"; standard= "TER"; canBeMid = false; caseValue = StreetType.Ter }
            { primary = "THROUGHWAY"; common = "THROUGHWAY"; standard= "TRWY"; canBeMid = false; caseValue = StreetType.Trwy }
            { primary = "TRACE"; common = "TRACE"; standard= "TRCE"; canBeMid = false; caseValue = StreetType.Trce }
            { primary = "TRACE"; common = "TRACES"; standard= "TRCE"; canBeMid = false; caseValue = StreetType.Trce }
            { primary = "TRACE"; common = "TRCE"; standard= "TRCE"; canBeMid = false; caseValue = StreetType.Trce }
            { primary = "TRACK"; common = "TRACK"; standard= "TRAK"; canBeMid = false; caseValue = StreetType.Trak }
            { primary = "TRACK"; common = "TRACKS"; standard= "TRAK"; canBeMid = false; caseValue = StreetType.Trak }
            { primary = "TRACK"; common = "TRAK"; standard= "TRAK"; canBeMid = false; caseValue = StreetType.Trak }
            { primary = "TRACK"; common = "TRK"; standard= "TRAK"; canBeMid = false; caseValue = StreetType.Trak }
            { primary = "TRACK"; common = "TRKS"; standard= "TRAK"; canBeMid = false; caseValue = StreetType.Trak }
            { primary = "TRAFFICWAY"; common = "TRAFFICWAY"; standard= "TRFY"; canBeMid = false; caseValue = StreetType.Trfy }
            { primary = "TRAIL"; common = "TRAIL"; standard= "TRL"; canBeMid = false; caseValue = StreetType.Trl }
            { primary = "TRAIL"; common = "TRAILS"; standard= "TRL"; canBeMid = false; caseValue = StreetType.Trl }
            { primary = "TRAIL"; common = "TRL"; standard= "TRL"; canBeMid = false; caseValue = StreetType.Trl }
            { primary = "TRAIL"; common = "TRLS"; standard= "TRL"; canBeMid = false; caseValue = StreetType.Trl }

            // kk:20191022 - MD4 has only 14 addresses with the street type "trailer" but over 95K addresses with "trailer" as a suite name.
            //{ primary = "TRAILER"; common = "TRAILER"; standard= "TRLR"; canBeMid = false; caseValue = StreetType.Trlr }
            //{ primary = "TRAILER"; common = "TRLR"; standard= "TRLR"; canBeMid = false; caseValue = StreetType.Trlr }
            //{ primary = "TRAILER"; common = "TRLRS"; standard= "TRLR"; canBeMid = false; caseValue = StreetType.Trlr }

            { primary = "TUNNEL"; common = "TUNEL"; standard= "TUNL"; canBeMid = false; caseValue = StreetType.Tunl }
            { primary = "TUNNEL"; common = "TUNL"; standard= "TUNL"; canBeMid = false; caseValue = StreetType.Tunl }
            { primary = "TUNNEL"; common = "TUNLS"; standard= "TUNL"; canBeMid = false; caseValue = StreetType.Tunl }
            { primary = "TUNNEL"; common = "TUNNEL"; standard= "TUNL"; canBeMid = false; caseValue = StreetType.Tunl }
            { primary = "TUNNEL"; common = "TUNNELS"; standard= "TUNL"; canBeMid = false; caseValue = StreetType.Tunl }
            { primary = "TUNNEL"; common = "TUNNL"; standard= "TUNL"; canBeMid = false; caseValue = StreetType.Tunl }
            { primary = "TURNPIKE"; common = "TRNPK"; standard= "TPKE"; canBeMid = false; caseValue = StreetType.Tpke }
            { primary = "TURNPIKE"; common = "TURNPIKE"; standard= "TPKE"; canBeMid = false; caseValue = StreetType.Tpke }
            { primary = "TURNPIKE"; common = "TURNPK"; standard= "TPKE"; canBeMid = false; caseValue = StreetType.Tpke }
            { primary = "UNDERPASS"; common = "UNDERPASS"; standard= "UPAS"; canBeMid = false; caseValue = StreetType.Upas }
            { primary = "UNION"; common = "UN"; standard= "UN"; canBeMid = false; caseValue = StreetType.Un }
            { primary = "UNION"; common = "UNION"; standard= "UN"; canBeMid = false; caseValue = StreetType.Un }
            { primary = "UNIONS"; common = "UNIONS"; standard= "UNS"; canBeMid = false; caseValue = StreetType.Uns }
            { primary = "VALLEY"; common = "VALLEY"; standard= "VLY"; canBeMid = false; caseValue = StreetType.Vly }
            { primary = "VALLEY"; common = "VALLY"; standard= "VLY"; canBeMid = false; caseValue = StreetType.Vly }
            { primary = "VALLEY"; common = "VLLY"; standard= "VLY"; canBeMid = false; caseValue = StreetType.Vly }
            { primary = "VALLEY"; common = "VLY"; standard= "VLY"; canBeMid = false; caseValue = StreetType.Vly }
            { primary = "VALLEYS"; common = "VALLEYS"; standard= "VLYS"; canBeMid = false; caseValue = StreetType.Vlys }
            { primary = "VALLEYS"; common = "VLYS"; standard= "VLYS"; canBeMid = false; caseValue = StreetType.Vlys }
            { primary = "VIADUCT"; common = "VDCT"; standard= "VIA"; canBeMid = false; caseValue = StreetType.Via }
            { primary = "VIADUCT"; common = "VIA"; standard= "VIA"; canBeMid = false; caseValue = StreetType.Via }
            { primary = "VIADUCT"; common = "VIADCT"; standard= "VIA"; canBeMid = false; caseValue = StreetType.Via }
            { primary = "VIADUCT"; common = "VIADUCT"; standard= "VIA"; canBeMid = false; caseValue = StreetType.Via }
            { primary = "VIEW"; common = "VIEW"; standard= "VW"; canBeMid = false; caseValue = StreetType.Vw }
            { primary = "VIEW"; common = "VW"; standard= "VW"; canBeMid = false; caseValue = StreetType.Vw }
            { primary = "VIEWS"; common = "VIEWS"; standard= "VWS"; canBeMid = false; caseValue = StreetType.Vws }
            { primary = "VIEWS"; common = "VWS"; standard= "VWS"; canBeMid = false; caseValue = StreetType.Vws }
            { primary = "VILLAGE"; common = "VILL"; standard= "VLG"; canBeMid = false; caseValue = StreetType.Vlg }
            { primary = "VILLAGE"; common = "VILLAG"; standard= "VLG"; canBeMid = false; caseValue = StreetType.Vlg }
            { primary = "VILLAGE"; common = "VILLAGE"; standard= "VLG"; canBeMid = false; caseValue = StreetType.Vlg }
            { primary = "VILLAGE"; common = "VILLG"; standard= "VLG"; canBeMid = false; caseValue = StreetType.Vlg }
            { primary = "VILLAGE"; common = "VILLIAGE"; standard= "VLG"; canBeMid = false; caseValue = StreetType.Vlg }
            { primary = "VILLAGE"; common = "VLG"; standard= "VLG"; canBeMid = false; caseValue = StreetType.Vlg }
            { primary = "VILLAGES"; common = "VILLAGES"; standard= "VLGS"; canBeMid = false; caseValue = StreetType.Vlgs }
            { primary = "VILLAGES"; common = "VLGS"; standard= "VLGS"; canBeMid = false; caseValue = StreetType.Vlgs }
            { primary = "VILLE"; common = "VILLE"; standard= "VL"; canBeMid = false; caseValue = StreetType.Vl }
            { primary = "VILLE"; common = "VL"; standard= "VL"; canBeMid = false; caseValue = StreetType.Vl }
            { primary = "VISTA"; common = "VIS"; standard= "VIS"; canBeMid = false; caseValue = StreetType.Vis }
            { primary = "VISTA"; common = "VIST"; standard= "VIS"; canBeMid = false; caseValue = StreetType.Vis }
            { primary = "VISTA"; common = "VISTA"; standard= "VIS"; canBeMid = false; caseValue = StreetType.Vis }
            { primary = "VISTA"; common = "VST"; standard= "VIS"; canBeMid = false; caseValue = StreetType.Vis }
            { primary = "VISTA"; common = "VSTA"; standard= "VIS"; canBeMid = false; caseValue = StreetType.Vis }
            { primary = "WALK"; common = "WALK"; standard= "WALK"; canBeMid = false; caseValue = StreetType.Walk }
            { primary = "WALKS"; common = "WALKS"; standard= "WALK"; canBeMid = false; caseValue = StreetType.Walk }
            { primary = "WALL"; common = "WALL"; standard= "WALL"; canBeMid = false; caseValue = StreetType.Wall }

            // Not used by MD
            //{ primary = "WAY"; common = "WY"; standard= "WAY"; canBeMid = false; caseValue = StreetType.Way }

            { primary = "WAY"; common = "WAY"; standard= "WAY"; canBeMid = false; caseValue = StreetType.Way }
            { primary = "WAYS"; common = "WAYS"; standard= "WAYS"; canBeMid = false; caseValue = StreetType.Ways }
            { primary = "WELL"; common = "WELL"; standard= "WL"; canBeMid = false; caseValue = StreetType.Wl }
            { primary = "WELLS"; common = "WELLS"; standard= "WLS"; canBeMid = false; caseValue = StreetType.Wls }
            { primary = "WELLS"; common = "WLS"; standard= "WLS"; canBeMid = false; caseValue = StreetType.Wls }

            // kk:20191209 - The street types below are manually added Spanish equivalents of street types. Keep adding as needed.
            { primary = "AVENIDA"; common = "AVENIDA"; standard= "AVENIDA"; canBeMid = false; caseValue = StreetType.Avenida }
            { primary = "CALLE"; common = "CALLE"; standard= "CALLE"; canBeMid = false; caseValue = StreetType.Calle }

        |]


    /// Words, which can be both street names and street types.
    /// If the original stree name is e.g. "CRESCENT DRIVE", then we want to be able to match by just "CRESCENT", but not just by "DR".
    static member streetNameOrTypeSet =
        [
            "BCH"
            "MDW"
            "CRES"
            "CIR"
        ]
        |> Set.ofList


    /// Occurrence of street types in MD4.
    static member streetTypeOccurrence =
        [
            ( "ST", 16753279 )
            ( "DR", 15715060 )
            ( "RD", 13922469 )
            ( "AVE", 11884062 )
            ( "LN", 5719349 )
            ( "CT", 4249990 )
            ( "CIR", 2030189 )
            ( "WAY", 1938933 )
            ( "PL", 1791887 )
            ( "BLVD", 983183 )
            ( "TRL", 795136 )
            ( "TER", 595732 )
            ( "LOOP", 275002 )
            ( "HWY", 265908 )
            ( "PKWY", 188220 )
            ( "CV", 167321 )
            ( "RUN", 99149 )
            ( "PIKE", 81514 )
            ( "RDG", 66241 )
            ( "PT", 60813 )
            ( "TRCE", 58335 )
            ( "PATH", 52303 )
            ( "PASS", 43107 )
            ( "XING", 41272 )
            ( "PARK", 39069 )
            ( "EXT", 37600 )
            ( "SQ", 36980 )
            ( "BND", 30680 )
            ( "HL", 25373 )
            ( "CRK", 23030 )
            ( "HTS", 22265 )
            ( "WALK", 21724 )
            ( "VW", 20427 )
            ( "TPKE", 20326 )
            ( "GLN", 19974 )
            ( "HOLW", 16560 )
            ( "CRES", 15816 )
            ( "LNDG", 14656 )
            ( "ROW", 14335 )
            ( "VIS", 13115 )
            ( "MNR", 12402 )
            ( "GRV", 10903 )
            ( "PLZ", 10717 )
            ( "EST", 10677 )
            ( "BR", 9373 )
            ( "ALY", 9316 )
            ( "LK", 7923 )
            ( "BLF", 7502 )
            ( "GRN", 7374 )
            ( "CRST", 6808 )
            ( "HLS", 6205 )
            ( "VLY", 6042 )
            ( "MDWS", 5319 )
            ( "CURV", 5246 )
            ( "VLG", 5187 )
            ( "SPUR", 5087 )
            ( "CYN", 5064 )
            ( "FRK", 4622 )
            ( "CMN", 4607 )
            ( "MDW", 4125 )
            ( "SPGS", 4098 )
            ( "FLS", 4090 )
            ( "FRST", 4050 )
            ( "KNL", 3944 )
            ( "OVAL", 3632 )
            ( "BRK", 2884 )
            ( "ISLE", 2817 )
            ( "IS", 2691 )
            ( "ESTS", 2552 )
            ( "GDNS", 2492 )
            ( "MEWS", 2459 )
            ( "HVN", 2411 )
            ( "SHRS", 2353 )
            ( "CRSE", 2217 )
            ( "MTN", 2207 )
            ( "RNCH", 2159 )
            ( "KNLS", 2082 )
            ( "STA", 2050 )
            ( "HBR", 2032 )
            ( "CMNS", 2024 )
            ( "PSGE", 2001 )
            ( "BCH", 1958 )
            ( "SPG", 1956 )
            ( "COR", 1884 )
            ( "PNES", 1870 )
            ( "FLD", 1828 )
            ( "RTE", 1712 )
            ( "ML", 1709 )
            ( "KY", 1586 )
            ( "PR", 1416 )
            ( "CLB", 1360 )
            ( "RIV", 1230 )
            ( "FWY", 1200 )
            ( "BRG", 1186 )
            ( "GDN", 1153 )
            ( "LDG", 1133 )
            ( "SMT", 1130 )
            ( "PNE", 1070 )
            ( "VIA", 1052 )
            ( "BYP", 1051 )
            ( "EXPY", 1028 )
            ( "CLF", 1024 )
            ( "STRA", 1022 )
            ( "TRAK", 865 )
            ( "DL", 859 )
            ( "SHR", 853 )
            ( "FLDS", 803 )
            ( "JCT", 706 )
            ( "GTWY", 692 )
            ( "ARC", 677 )
            ( "CTS", 675 )
            ( "MALL", 630 )
            ( "LKS", 607 )
            ( "XRD", 580 )
            ( "STRM", 570 )
            ( "FRD", 543 )
            ( "FRG", 511 )
            ( "RUE", 511 )
            ( "FRY", 451 )
            ( "FLT", 444 )
            ( "RST", 366 )
            ( "FLTS", 365 )
            ( "CSWY", 363 )
            ( "ORCH", 345 )
            ( "BYU", 340 )
            ( "CORS", 326 )
            ( "PRT", 314 )
            ( "CTR", 313 )
            ( "WLS", 305 )
            ( "BTM", 288 )
            ( "NCK", 267 )
            ( "ANX", 266 )
            ( "SHLS", 256 )
            ( "FALL", 255 )
            ( "RADL", 249 )
            ( "CP", 248 )
            ( "PLN", 246 )
            ( "BLFS", 220 )
            ( "MLS", 216 )
            ( "WL", 211 )
            ( "MT", 203 )
            ( "SKWY", 199 )
            ( "DV", 197 )
            ( "CLFS", 186 )
            ( "LAND", 169 )
            ( "INLT", 164 )
            ( "LGT", 159 )
            ( "PLNS", 157 )
            ( "TRFY", 150 )
            ( "CPE", 147 )
            ( "TRWY", 138 )
            ( "FT", 109 )
            ( "FRKS", 107 )
            ( "DM", 106 )
            ( "LGTS", 106 )
            ( "SHL", 97 )
            ( "BRKS", 75 )
            ( "CVS", 70 )
            ( "MTWY", 65 )
            ( "GRNS", 60 )
            ( "RPDS", 48 )
            ( "OPAS", 45 )
            ( "PTS", 42 )
            ( "RDS", 40 )
            ( "VL", 35 )
            ( "UN", 31 )
            ( "GRVS", 30 )
            ( "RDGS", 27 )
            ( "VWS", 26 )
            ( "MSN", 24 )
            ( "WALL", 21 )
            ( "LF", 18 )
            ( "RAMP", 17 )
            ( "TRLR", 17 )
            ( "XRDS", 14 )
            ( "DRS", 13 )
            ( "WAYS", 12 )
            ( "KYS", 11 )
            ( "LCK", 10 )
            ( "STS", 6 )
            ( "UPAS", 4 )
            ( "ISS", 2 )
        ]
