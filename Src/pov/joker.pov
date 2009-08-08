#include "colors.inc"
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
  location <0, 0, -18>
//  location <0, -1, -11>
  look_at <0,0,0>
  angle 15
}

//background { color White }
//box { <-5, -5, 7>, <5, 5, 7.1> pigment { color White } }

light_source { <-20, 30, -30> color White*.5 }
light_source { <-30, 30, -30> color White*.5 }
light_source { <-20, 20, -30> color White*.5 }
light_source { <-30, 20, -30> color White*.5 }

#declare sph = .2;

union {
    sphere_sweep {
        b_spline
        7,
    
        <-.85, -1, 0>, 1.25*sph,
        <-.85, 1, 0>, 1.25*sph,
        <.7, 1, 0>, sph,
        <.7, 0, 0>, sph,
        <0, 0, 0>, sph,
        <0, -.5, 0>, 0.8*sph,
        <0, -.7, 0>, 0.8*sph
    }    

    sphere { <0, -1, 0>, 1.25*sph }

    material {
        texture {
            // pigment { color rgbt<1, .5, 1, .5> }
            T_Silver_2A
            normal { bumps 0.15 scale 1 translate 1*x+1*y }
            finish { phong .5 }
        }
    }
}

merge {
    cylinder {
        <0, 0, .2>, <0, 0, .3>, 1.5
        texture { T_Gold_1D }
        normal { bumps .05 scale .01 }
    }
    torus { 1.5, .1 rotate 90*x translate .25*z }
    texture { T_Gold_1D }
}