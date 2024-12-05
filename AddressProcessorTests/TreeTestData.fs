﻿namespace Softellect.AddressProcessorTests

module TreeTestData =

    /// Top 100 zip codes with the largest number of streets.
    let zipCodes =
        [
            "71909"
            "71913"
            "30512"
            "28734"
            "08831"
            "84120"
            "84121"
            "30143"
            "77494"
            "71901"
            "60190"
            "37110"
            "95667"
            "37876"
            "81007"
            "32162"
            "75070"
            "27030"
            "92880"
            "30349"
            "77351"
            "28906"
            "28655"
            "77479"
            "28645"
            "43230"
            "77573"
            "32259"
            "75071"
            "77433"
            "02360"
            "72653"
            "30907"
            "84119"
            "60134"
            "30022"
            "15642"
            "20854"
            "30004"
            "77429"
            "77584"
            "28792"
            "28786"
            "27587"
            "77382"
            "92592"
            "60010"
            "21502"
            "63376"
            "45011"
            "77449"
            "77379"
            "08753"
            "84128"
            "21093"
            "31313"
            "76513"
            "26554"
            "77459"
            "91320"
            "95747"
            "77381"
            "30135"
            "34953"
            "89031"
            "75002"
            "28715"
            "55110"
            "28607"
            "30041"
            "95762"
            "28714"
            "45424"
            "38017"
            "30540"
            "15601"
            "30062"
            "37862"
            "99654"
            "30043"
            "30024"
            "38135"
            "27284"
            "29707"
            "30127"
            "29720"
            "28173"
            "46350"
            "28739"
            "95843"
            "30040"
            "30533"
            "78130"
            "98387"
            "29072"
            "30721"
            "28461"
            "79928"
            "29681"
            "28625"
        ]
        //|> List.take 10


    /// Random streets from top 100 zips (above).
    /// Correct / Removed Space / Changed one character
    let streets100 =
        [
            "DRAKE MTN LN", "DRAKEMTN LN", "ERAKEMTN LN"
            "GRANDE LN", "GRANDELN", "GSANDELN"
            "158 ST S", "158ST S", "159ST S"
            "WILD ROSE LN", "WILDROSE LN", "WILEROSE LN"
            "W EAGLE PARK LN", "WEAGLE PARK LN", "WEAGME PARK LN"
            "SPLINTER ROCK WAY", "SPLINTERROCK WAY", "SPLINUERROCK WAY"
            "SWIFTSTREAM PL", "SWIFTSTREAMPL", "SWIFTSUREAMPL"
            "KATIE EMMA DR", "KATIEEMMA DR", "KATIEEMNA DR"
            "CROAISDALE CT", "CROAISDALECT", "CROAISDAMECT"
            "LINDSAY CT", "LINDSAYCT", "MINDSAYCT"
            "ELDERBERRY CT", "ELDERBERRYCT", "EMDERBERRYCT"
            "GRAHL ST", "GRAHLST", "GRBHLST"
            "TO 1519 148 ST CT E", "TO1519 148 ST CT E", "TO1619 148 ST CT E"
            "5 N SUSSEX WAY", "5N SUSSEX WAY", "5N SVSSEX WAY"
            "OLD SAMPSON RD", "OLDSAMPSON RD", "OLDSANPSON RD"
            "PIONEER TRL DR", "PIONEERTRL DR", "PIONEESTRL DR"
            "CARTAYA LN", "CARTAYALN", "CARTAYAMN"
            "CREEKS EDGE WAY", "CREEKSEDGE WAY", "CREEKSEDHE WAY"
            "SCORPIO PL", "SCORPIOPL", "TCORPIOPL"
            "DEB MAR WOODS PL", "DEBMAR WOODS PL", "DFBMAR WOODS PL"
            "7 ST CT", "7ST CT", "7SU CT"
            "FAIR OAK TER", "FAIROAK TER", "FAISOAK TER"
            "BUCKHURST TRL", "BUCKHURSTTRL", "BUCKIURSTTRL"
            "BOMAR RD", "BOMARRD", "BOMARSD"
            "NEW PT CV", "NEWPT CV", "NEWPT DV"
            "CLOVERGATE CIR", "CLOVERGATECIR", "CLOVERGBTECIR"
            "ALEXANDRIA CT", "ALEXANDRIACT", "ALEXANDRJACT"
            "APPALACHIAN ESTS RD", "APPALACHIANESTS RD", "APPALACHIBNESTS RD"
            "ECHO LKS CIR", "ECHOLKS CIR", "ECHOLKS CIS"
            "TRAILWOODS DR", "TRAILWOODSDR", "TRAILWOODSDS"
            "GEORGEANNA ST", "GEORGEANNAST", "HEORGEANNAST"
            "ROCK RDG RD CC 719", "ROCKRDG RD CC 719", "RPCKRDG RD CC 719"
            "LK VW LN", "LKVW LN", "LKWW LN"
            "202 N OLD NASSAU RD", "202N OLD NASSAU RD", "202O OLD NASSAU RD"
            "SEABROOK CT", "SEABROOKCT", "SEABSOOKCT"
            "HAMPTON FRST PL", "HAMPTONFRST PL", "HAMPTPNFRST PL"
            "ROUGHRIDER CT", "ROUGHRIDERCT", "ROUGHRJDERCT"
            "LAKERIDGE OAKS DR", "LAKERIDGEOAKS DR", "LAKERIDHEOAKS DR"
            "GLENN MOOR DR", "GLENNMOOR DR", "GLENNMOOS DR"
            "16 AVE S", "16AVE S", "26AVE S"
            "VAN TUYL PKWY", "VANTUYL PKWY", "VBNTUYL PKWY"
            "HEATH RIV LN", "HEATHRIV LN", "HEBTHRIV LN"
            "CR 101 D", "CR101 D", "CR111 D"
            "FOX TROT LN", "FOXTROT LN", "FOXTSOT LN"
            "ADRIENNE DR", "ADRIENNEDR", "ADRIEONEDR"
            "WATAUGA DR", "WATAUGADR", "WATAUGBDR"
            "CRST MAR CIR", "CRSTMAR CIR", "CRSTMAR!CIR"
            "FRED MCGAHA DR", "FREDMCGAHA DR", "FREDMCGAIA DR"
            "MARINA BAY DR 12207", "MARINABAY DR 12207", "MARINABAY!DR 12207"
            "INDIAN WALK RD", "INDIANWALK RD", "INDIANWALK!RD"
            "GRINDELWALD DR", "GRINDELWALDDR", "GRINDELWALDER"
            "DIMEBOX DR", "DIMEBOXDR", "EIMEBOXDR"
            "W CROWNPOINTE DR", "WCROWNPOINTE DR", "WDROWNPOINTE DR"
            "CAVENDISH PL", "CAVENDISHPL", "CAWENDISHPL"
            "KELLY PL", "KELLYPL", "KELMYPL"
            "ROLLING RD", "ROLLINGRD", "ROLLJNGRD"
            "OSAGE LN", "OSAGELN", "OSAGEMN"
            "EVERNIA CT", "EVERNIACT", "EVERNIBCT"
            "MAKANDA DR", "MAKANDADR", "MAKANDAER"
            "PLANTATION", "PLANTATIONZ", "PLANTATIPNZ"
            "FINLEY CT", "FINLEYCT", "GINLEYCT"
            "WOODS EDGE RD", "WOODSEDGE RD", "WPODSEDGE RD"
            "RAPLEY RDG LN", "RAPLEYRDG LN", "RAQLEYRDG LN"
            "JACKPINE DR", "JACKPINEDR", "JACLPINEDR"
            "TETHERED VINE PL", "TETHEREDVINE PL", "TETHFREDVINE PL"
            "BREEZEWOOD WAY", "BREEZEWOODWAY", "BREEZFWOODWAY"
            "S OLD RDG CIR", "SOLD RDG CIR", "SOLD REG CIR"
            "EAGLE GLN RD", "EAGLEGLN RD", "EAGLEGLO RD"
            "N LILLY DR", "NLILLY DR", "NLILLY DS"
            "W DEAN DR", "WDEAN DR", "XDEAN DR"
            "PINETOP GLN LN", "PINETOPGLN LN", "PJNETOPGLN LN"
            "CHADWICK LKS DR", "CHADWICKLKS DR", "CHBDWICKLKS DR"
            "BARNACLE RD", "BARNACLERD", "BAROACLERD"
            "CANTATA DR", "CANTATADR", "CANTBTADR"
            "N HORSESHOE BND", "NHORSESHOE BND", "NHORSFSHOE BND"
            "PARK PL LN", "PARKPL LN", "PARKPL!LN"
            "COLINA CORONA DR", "COLINACORONA DR", "COLINACPRONA DR"
            "RUELLA PL", "RUELLAPL", "SUELLAPL"
            "BIG SANDY CT", "BIGSANDY CT", "BJGSANDY CT"
            "RIVERMIST PT", "RIVERMISTPT", "RIWERMISTPT"
            "STONEBROOK FARMS DR", "STONEBROOKFARMS DR", "STOOEBROOKFARMS DR"
            "LILLIAN CT", "LILLIANCT", "LILLJANCT"
            "LOWER TRACTION RD", "LOWERTRACTION RD", "LOWERURACTION RD"
            "BRADLEY CV", "BRADLEYCV", "BRADLEZCV"
            "MIDDLEWOOD MNR LN", "MIDDLEWOODMNR LN", "MIDDLEWPODMNR LN"
            "MISTWOOD DR", "MISTWOODDR", "MISTWOODER"
            "BONNERS PARK CT", "BONNERSPARK CT", "BONNERSPASK CT"
            "GRAND VW ST", "GRANDVW ST", "HRANDVW ST"
            "CEMETERY LN", "CEMETERYLN", "CFMETERYLN"
            "WOOD RIDE LN", "WOODRIDE LN", "WOPDRIDE LN"
            "TERRYS GAP RD", "TERRYSGAP RD", "TERSYSGAP RD"
            "KARR CT", "KARRCT", "KARRDT"
            "WOOD VLY DR", "WOODVLY DR", "WOODVMY DR"
            "ST ANDREWS CIR SE", "STANDREWS CIR SE", "STANDRFWS CIR SE"
            "OAKCREST LN", "OAKCRESTLN", "OAKCRESULN"
            "SW JACKSONVILLE AVE", "SWJACKSONVILLE AVE", "SWJACKSOOVILLE AVE"
            "CHARLOTTE CT", "CHARLOTTECT", "CHARLOTTEDT"
            "CHAMPIONS CLB CT", "CHAMPIONSCLB CT", "CHAMPIONSCMB CT"
            "DORCHESTER WAY", "DORCHESTERWAY", "DORCHESTERWBY"
            "BLACKFOOT ST", "BLACKFOOTST", "CLACKFOOTST"
        ]
        //|> List.take 10


    /// Random streets from top 10 zips (above).
    let streets10 =
        [
            "S BASS BAY", "SBASS BAY", "TBASS BAY"
            "CYPRESS DR", "CYPRESSDR", "CZPRESSDR"
            "LOPES CIR", "LOPESCIR", "LOQESCIR"
            "CONTINENTAL DR", "CONTINENTALDR", "CONUINENTALDR"
            "KALMIA PLZ", "KALMIAPLZ", "KALMJAPLZ"
            "W MAPLE MDWS DR", "WMAPLE MDWS DR", "WMAPLF MDWS DR"
            "ENCANTADO LN", "ENCANTADOLN", "ENCANTBDOLN"
            "LK VIS DR", "LKVIS DR", "LKVIS DS"
            "SALIX PLZ", "SALIXPLZ", "TALIXPLZ"
            "WATER OAK CIR", "WATEROAK CIR", "WBTEROAK CIR"
            "QUAIL CRK RD", "QUAILCRK RD", "QUBILCRK RD"
            "GOODVIEW TRL", "GOODVIEWTRL", "GOOEVIEWTRL"
            "GEORGIA RD", "GEORGIARD", "GEORHIARD"
            "CHOCTAW RDG", "CHOCTAWRDG", "CHOCTBWRDG"
            "SUNNY VW DR", "SUNNYVW DR", "SUNNYVX DR"
            "MEMORIAL LN", "MEMORIALLN", "MEMORIAMLN"
            "CORIA TRCE", "CORIATRCE", "CORIATRCF"
            "GRAPEVINE TRL", "GRAPEVINETRL", "GRAPEVINEURL"
            "FRST HLS DR", "FRSTHLS DR", "GRSTHLS DR"
            "DERECHO LN", "DERECHOLN", "DFRECHOLN"
            "LEANING TREE LN", "LEANINGTREE LN", "LEBNINGTREE LN"
            "BLAKE ST", "BLAKEST", "BLALEST"
            "ORCH RDG TRL", "ORCHRDG TRL", "ORCHSDG TRL"
            "SCOOTER LN", "SCOOTERLN", "SCOOTFRLN"
            "REBECCA MDW FLS DR", "REBECCAMDW FLS DR", "REBECCBMDW FLS DR"
            "ROBERTS DR", "ROBERTSDR", "ROBERTSER"
            "E CYPRESS WAY", "ECYPRESS WAY", "ECYPRESS!WAY"
            "MALLARD BAY LN", "MALLARDBAY LN", "MALLARDBAZ LN"
            "ERMALINDA PL", "ERMALINDAPL", "ERMALINDAPM"
            "MORNING STAR LN", "MORNINGSTAR LN", "MORNINGSTAR!LN"
            "HAMILTON PL", "HAMILTONPL", "IAMILTONPL"
            "ARBOR ST", "ARBORST", "ASBORST"
            "BUTTONWOOD CT", "BUTTONWOODCT", "BUUTONWOODCT"
            "TERRELL RD", "TERRELLRD", "TERSELLRD"
            "SAMANTHA RD", "SAMANTHARD", "SAMAOTHARD"
            "COMMANDMENT LN", "COMMANDMENTLN", "COMMAODMENTLN"
            "HAWTHORNE GDN WAY", "HAWTHORNEGDN WAY", "HAWTHOSNEGDN WAY"
            "INICIADOR WAY", "INICIADORWAY", "INICIADPRWAY"
            "90 N GLOUCESTER WAY", "90N GLOUCESTER WAY", "90N GLOUDESTER WAY"
            "LK FRST SHRS DR", "LKFRST SHRS DR", "LKFRST SHSS DR"
            "CLEVELAND ST", "CLEVELANDST", "CLEVELANDSU"
            "CHEROKEE WAY", "CHEROKEEWAY", "DHEROKEEWAY"
            "HOLT DR", "HOLTDR", "HPLTDR"
            "VUESTA WAY", "VUESTAWAY", "VUFSTAWAY"
            "S NANTUCKET DR", "SNANTUCKET DR", "SNAOTUCKET DR"
            "WOODSTREAM PT", "WOODSTREAMPT", "WOODTTREAMPT"
            "YOUNG CANE CRK RD", "YOUNGCANE CRK RD", "YOUNGDANE CRK RD"
            "HAMILTON VW CV", "HAMILTONVW CV", "HAMILTPNVW CV"
            "SHADY HTS RD", "SHADYHTS RD", "SHADYHTT RD"
            "S EARLY DUKE ST SC", "SEARLY DUKE ST SC", "SEARLY DVKE ST SC"
            "CHINQUAPIN MTN RD", "CHINQUAPINMTN RD", "CHINQUAPIOMTN RD"
            "UTRERA LN", "UTRERALN", "VTRERALN"
            "414 N OXFORD LN", "414N OXFORD LN", "424N OXFORD LN"
            "MCCARTER HL", "MCCARTERHL", "MCDARTERHL"
            "CRYSTAL LEAF LN", "CRYSTALLEAF LN", "CRYTTALLEAF LN"
            "S IRON BLOSSOM CIR", "SIRON BLOSSOM CIR", "SIROO BLOSSOM CIR"
            "NEW BEDFORD LN", "NEWBEDFORD LN", "NEWBEEFORD LN"
            "CONSOLA PL", "CONSOLAPL", "CONSOLBPL"
            "LITTLE HENDRICKS MTN CIR 20940", "LITTLEHENDRICKS MTN CIR 20940", "LITTLEHFNDRICKS MTN CIR 20940"
            "NATURAL DR", "NATURALDR", "NATURALDS"
            "WAYNES HBR TRL", "WAYNESHBR TRL", "WAYNESHBR!TRL"
            "9 N SUSSEX WAY", "9N SUSSEX WAY", "9N SUSSEX XAY"
            "NEWBY LN", "NEWBYLN", "OEWBYLN"
            "BYRD GAP RD", "BYRDGAP RD", "BZRDGAP RD"
            "MOON RDG CT", "MOONRDG CT", "MOPNRDG CT"
            "RUSSELL SCENIC HWY", "RUSSELLSCENIC HWY", "RUSTELLSCENIC HWY"
            "YORK PL", "YORKPL", "YORKQL"
            "BRENDLE CV RD", "BRENDLECV RD", "BRENDMECV RD"
            "E 7745 S", "E7745 S", "E7745 T"
            "BOAZ ST", "BOAZST", "COAZST"
            "PEANUT LN", "PEANUTLN", "PFANUTLN"
            "AYLEN VLG LN", "AYLENVLG LN", "AYMENVLG LN"
            "SHENANDOAH FARMS RD", "SHENANDOAHFARMS RD", "SHEOANDOAHFARMS RD"
            "S 1935 E", "S1935 E", "S1936 E"
            "HAWKSTONE CT", "HAWKSTONECT", "HAWKSUONECT"
            "NOTTELY RIV PL", "NOTTELYRIV PL", "NOTTELZRIV PL"
            "TWISTED PNE LN", "TWISTEDPNE LN", "TWISTEDQNE LN"
            "BRG CRK LN", "BRGCRK LN", "BRGCRK LO"
            "343 N OLD NASSAU RD", "343N OLD NASSAU RD", "343N OLD OASSAU RD"
            "S WILLOW RDG RD", "SWILLOW RDG RD", "SWILLOW RDH RD"
            "LANDOVER HLS LN", "LANDOVERHLS LN", "LANDOVERHLS!LN"
            "CANACAUGHT PL", "CANACAUGHTPL", "DANACAUGHTPL"
            "RUNNING DEER DR", "RUNNINGDEER DR", "RVNNINGDEER DR"
            "PERCH LN", "PERCHLN", "PESCHLN"
            "SUNSET CLF DR", "SUNSETCLF DR", "SUNTETCLF DR"
            "561 N TILTON WAY", "561N TILTON WAY", "561N!TILTON WAY"
            "LANDRUM RD", "LANDRUMRD", "LANDRVMRD"
            "HAYUCO WAY", "HAYUCOWAY", "HAYUCOXAY"
            "PINEHURST RD", "PINEHURSTRD", "PINEHURTTRD"
            "ANTEBELLUM AVE", "ANTEBELLUMAVE", "ANTEBELLVMAVE"
            "SPARTAN PL", "SPARTANPL", "TPARTANPL"
            "DETROIT ST", "DETROITST", "DFTROITST"
            "164 N PORTLAND LN", "164N PORTLAND LN", "165N PORTLAND LN"
            "CYN WREN DR", "CYNWREN DR", "CYNXREN DR"
            "NOTTELYWOOD W", "NOTTELYWOODW", "NOTTFLYWOODW"
            "TINAS CT", "TINASCT", "TINASDT"
            "OLD SMOKEY RD", "OLDSMOKEY RD", "OLDSMOLEY RD"
            "377 N OLD NASSAU RD", "377N OLD NASSAU RD", "377N OLE NASSAU RD"
            "BOLTON TRL LN", "BOLTONTRL LN", "BOLTONTRM LN"
            "HAWTHORNE LN", "HAWTHORNELN", "HAWTHORNEMN"
        ]
