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
//box { <-5, -5, .1>, <5, 5, 100> pigment { color White*2 } }

light_source { <-10, 10, -30> color White*.5 }
light_source { <-30, 10, -30> color White*.5 }
light_source { <-10, 30, -30> color White*.5 }
light_source { <-30, 30, -30> color White*.5 }

#declare sn = function(x) { sin(x*pi/180) }
#declare cs = function(x) { cos(x*pi/180) }

union {
    prism {
        linear_sweep linear_spline -.075, .075, 4,
        <0.7/sqrt(2), -0.7/sqrt(2)>, <0.5/sqrt(2), -2/sqrt(2)>,
        <1.8/sqrt(2), -1.8/sqrt(2)>, <0.7/sqrt(2), -0.7/sqrt(2)>
        rotate -90*x
    }

    difference {
        merge {
            difference {
                cylinder { <0, 0, -.075>, <0, 0, .075>, 1.5 }
                cylinder { <0, 0, -1>, <0, 0, 1>, 1 }
            }
        }
        prism {
            linear_sweep linear_spline -1, 1, 4,
            <0, 0>, <100*cs(240), 100*sn(240)>, <100*cs(315), 100*sn(315)>, <0, 0>
            rotate -90*x
        }
    }
    material {
        texture {
            pigment { color rgb<.7, .3, .8> }
            normal { wrinkles .15 scale .2 rotate 45*x }
            finish { phong .5 }
        }
    }
    //translate .1*x - .1*y + .1*z
}

merge {

    difference {
        merge {
            torus { 1.5, .1  rotate 90*x }
            torus { 1, .1  rotate 90*x }
        }
        prism {
            linear_sweep linear_spline -1, 1, 4,
            <0, 0>, <100*cs(240), 100*sn(240)>, <100*cs(315), 100*sn(315)>, <0, 0>
            rotate -90*x
        }
    }

    sphere { <cs(240), sn(240), 0>, .1 }
    sphere { <1.5*cs(240), 1.5*sn(240), 0>, .1 }
    cylinder { <cs(240), sn(240), 0>, <1.5*cs(240), 1.5*sn(240), 0>, .1 }

    sphere { <1/sqrt(2), -1/sqrt(2), 0>, .1 }
    sphere { <0.7/sqrt(2), -0.7/sqrt(2), 0>, .1 }
    cylinder { <1/sqrt(2), -1/sqrt(2), 0>, <0.7/sqrt(2), -0.7/sqrt(2), 0>, .1 }

    sphere { <0.5/sqrt(2), -2/sqrt(2), 0>, .1 }
    cylinder { <0.7/sqrt(2), -0.7/sqrt(2), 0>, <0.5/sqrt(2), -2/sqrt(2), 0>, .1 }

    sphere { <1.8/sqrt(2), -1.8/sqrt(2), 0>, .1 }
    cylinder { <0.5/sqrt(2), -2/sqrt(2), 0>, <1.8/sqrt(2), -1.8/sqrt(2), 0>, .1 }

    sphere { <1.5/sqrt(2), -1.5/sqrt(2), 0>, .1 }
    cylinder { <1.8/sqrt(2), -1.8/sqrt(2), 0>, <1.5/sqrt(2), -1.5/sqrt(2), 0>, .1 }

    material {
        texture {
            pigment { color rgb<1, .5, 1> }
            normal { wrinkles .2 scale .1 rotate 45*x }
            finish { phong .7 }
        }
    }
    //translate .1*x - .1*y + .1*z
}

/*
cylinder { <0,0,0>, <100,0,0>, .1 pigment { Red } }
cylinder { <0,0,0>, <0,100,0>, .1 pigment { Green } }
cylinder { <0,0,0>, <0,0,100>, .1 pigment { Blue } }
*/
