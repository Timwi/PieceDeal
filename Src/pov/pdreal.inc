
#include "colors.inc"

/*
global_settings {
    assumed_gamma 1
    max_trace_level 30
    radiosity {
        count 50
        error_bound 2
        recursion_limit 2
        nearest_count 8
        brightness 1
        normal on
    }
}
*/

#declare xx = 0;
#while (xx < 10)
    #declare yy = 0;
    #while (yy < 10)
        light_source { <-30 + 2*xx + 2*yy, 20 + 2*xx - 2*yy, -30> color White/50 }
        #declare yy = yy + 1;
    #end
    #declare xx = xx + 1;
#end
