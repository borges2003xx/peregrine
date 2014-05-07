// -*- tab-width: 4; Mode: C++; c-basic-offset: 4; indent-tabs-mode: nil -*-

// This file is just a placeholder for your configuration file.  If
// you wish to change any of the setup parameters from their default
// values, place the appropriate #define statements here.

// If you used to define your CONFIG_APM_HARDWARE setting here, it is no
// longer valid! You should switch to using CONFIG_HAL_BOARD via the HAL_BOARD
// flag in your local config.mk instead.

// The following are the recommended settings for Xplane
// simulation. Remove the leading "/* and trailing "*/" to enable:

// Comment out HIL_MODE for none simulated compile
#define HIL_MODE            HIL_MODE_ATTITUDE
#define SOAR_ACTIVE 0    // Default - disapbled
#define THERMAL_VSPEED 2 // Default - 2 m/s


/*
 *  // HIL_MODE SELECTION
 *  //
 *  // Mavlink supports
 *  // 1. HIL_MODE_ATTITUDE : simulated position, airspeed, and attitude
 *  // 2. HIL_MODE_SENSORS: full sensor simulation
 *  //#define HIL_MODE            HIL_MODE_ATTITUDEs
 *
 */

float r3;
int   thermal_delay;
int   thermal_delay_target;
int   delay_min = 20;
int   delay_max = 80;
float angle_sx = -1;
float angle_dx =1;
int   actual_climb_rate;
int   old_climb_rate;
int   delta_roll = 1;

