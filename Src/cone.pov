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
  location <7, 0, -10>
//  location <0, -1, -11>
  look_at <0,-.1,0>
  angle 15
}

//background { color White }

light_source {
  <-12, 12, -15>
  color White*2
}

#declare sph = .05;

merge {
    cone {
        <0, 1 - 4*sph/sqrt(5), 0>, 2*sph/sqrt(5)
        <0, -1 + sph + sph/sqrt(5), 0>, 1 - (sph + sph/sqrt(5))/2
    }
    sphere {
        <0, 1 - 5*sph/sqrt(5), 0>, sph
    }
    torus {
        1 - sph/2 - 5/2*sph/sqrt(5),
        sph
        translate (-1 + sph)*y
    }
    cylinder {
        <0, -1, 0>, <0, -1 + 2*sph, 0>, 1 - sph/2 - 5/2*sph/sqrt(5)
    }

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
}
*/



















































merge {
    cone {
        <0, 1, 0>, 0
        <0, -1, 0>, 1
    }

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
}
