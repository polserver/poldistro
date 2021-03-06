#include "clib/stl_inc.h"
#include "charactr.h"
#include "client.h"
#include "msghandl.h"
#include "umsgtype.h"
#include "polcfg.h"
#include "scrstore.h"

void handle_help( Client* client, MSG9B_HELP* msg )
{
    ref_ptr<EScriptProgram> prog = find_script( "misc/help", 
                                                true, 
                                                config.cache_interactive_scripts );
    if (prog.get() != NULL)
    {
        client->chr->start_script( prog.get(), false );
    }
}
MESSAGE_HANDLER( MSG9B_HELP, handle_help );
