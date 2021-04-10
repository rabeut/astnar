This folder contains the source-code and makefile to build a modified version of vinci (originally developed by Benno Bueeler and Andreas Enge).
Details on the changes are given below. 
The modification is released under the GNU General Public License 3. 


Changes made to the original version:
Similar to the original version by Benno Bueeler and Andreas Enge, the tool computes the volume of a convex polytope given as a set of hyperplanes.
The following changes have been made compared to the original version (in accordance with the GNU General Public License 2 of the original version by Bueeler and Enge):

- The output behavior of the tool is simplified. The Debug outputs and information regarding the ongoing computation are omitted. Instead the tool simply outputs the computed volume in decimal representation. 
  This simplification allows external tools to simply read the standard output of the program to obtain the computed volume. 

- The modified version always uses the recursive algorithm due to Lassere. The support for all other algorithm (originally supported by vinci) is removed. 
  The implementation of the Lassere method is the same as in the original version.
  The input of a polytope must be given via hyperplanes opposed to vertices. 