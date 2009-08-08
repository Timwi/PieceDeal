#include "colors.inc"
#include "textures.inc"
#include "metals.inc"

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

camera {
  location <0, 0, -11.3>
  look_at <0, 0, 0>
  angle 15
}

//background { color White }
//box { <-2, -2, 5>, < 2,  2, 5.1> pigment { color White } }

light_source {
  <-12, 12, -15>
  color White*2
}

#declare Rmaj = 1;
#declare Rmin = .1;

isosurface {
    function {
        pow (Rmaj - pow(pow(x,6) + pow(y,6), 1/6), 6)
        + pow (z, 6) - pow (Rmin, 6)
    }
    threshold 0
    max_gradient 2.323
    accuracy .01
    contained_by {box {<-Rmaj-Rmin-.001, -Rmaj-Rmin-.001, -Rmin-.001>,
                       < Rmaj+Rmin+.001,  Rmaj+Rmin+.001,  Rmin+.001>}}
    /*
    texture {
        pigment { color Silver }
        finish {
            ambient 0 diffuse 1
            specular 1 roughness 0.02
            brilliance 2
        }
    }
    */
    texture { T_Silver_1E }
}
