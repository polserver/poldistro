use uo;
use util;

function GetModAmount( magery ) 

    local mod_amount := randomint(10) + (magery/6);
    return mod_amount;

endfunction

function GetModDuration( magery )

    local duration := 60 + ((magery / 15) * 60);
    return (duration);

endfunction

function CanMod(who, stat)

	local mod;

	case (stat)
        	"str": mod := "#ss";
        	"dex" : mod := "#dd";
        	"int" : mod := "#ii";
        	"ar"  : mod := "#aa";
        	"p"  : mod := "#pp";
	endcase

	if (getobjproperty(who, mod))
        	return 0;
	else
        	return 1;
	endif

endfunction

function DoTempMod(who, stat, amt, duration)

	local mod;

	case (stat)
        	"str": 
			mod := "#ss";
			who.strength_mod := who.strength_mod + amt;
        	"dex" : 
			mod := "#dd";
			who.dexterity_mod := who.dexterity_mod + amt;
        	"int" : 
			mod := "#ii";
			who.intelligence_mod := who.intelligence_mod + amt;
        	"ar"  : 
			mod := "#aa";
			who.ar_mod := who.ar_mod + amt;
		"p"   :
			mod := "#pp";
			who.SetParalyzed(1);
	endcase

	SetObjProperty(who,mod,1);

	local parms := {};
	parms[2] := who;
	parms[1] := stat;
	parms[3] := amt;
	parms[4] := duration;

	start_script("::/misc/unmod", parms);

endfunction
