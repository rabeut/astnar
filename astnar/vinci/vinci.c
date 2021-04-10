/****************************************************************************************/
/*                                                                                      */
/* Authors: Raven Beutner                                                               */
/*                                                                                      */
/* Last Changes: April 4, 2021                                                          */
/*                                                                                      */
/****************************************************************************************/

/****************************************************************************************/
/* This is a modifed version of the tool VINCI originall developed by Benno Bueeler     */
/* and Andreas Enge. The original authors are named in all files that were modifed      */
/* The main changes are concered with simplyinf the output format of                    */
/* the tool and removing not-needed features. This modifed version is used by           */
/* the tool ASTNAR for verify termination of non-affine program.                        */ 
/****************************************************************************************/

#include "vinci.h"


int main (int argc, char *argv [])
{  
   rational   volume;

   volume_lasserre_file (&volume, "vol.ine");

   printf ("%f", volume);

   return 0;
}

/****************************************************************************************/
