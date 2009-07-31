#include "colors.inc"

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
  location <7, 5, -10>
  look_at <0, 0, 0>
  angle 15
}

//background { color White }

light_source {
  <-12, 12, -15>
  color White*2
}

#declare sph = .4;

blob {
    cylinder { <-1, 0, 0>, <1, 0, 0>, sph, 2 }
    cylinder { <0, -1, 0>, <0, 1, 0>, sph, 2 }
    cylinder { <0, 0, -1>, <0, 0, 1>, sph, 2 }

    material {
        texture {
//            pigment { color rgbt<.05,1,.2,0.5> }
//            pigment { color rgbt<1,0,0,0.5> }
//            pigment { color rgbt<.25,.5,1,0.5> }
            pigment { color rgbt<1,1,.15,0.5> }
            normal { bumps 0.15 scale 1 translate 1*x+1*y }
            finish { phong .5 }
        }
    }
    
    rotate -10*z
}
