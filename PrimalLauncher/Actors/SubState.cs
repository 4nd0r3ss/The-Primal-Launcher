/* 
Copyright (C) 2022 Andreus Faria

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;

namespace PrimalLauncher
{
    [Serializable]
    public class SubState
    {
        #region Battle related
        //From http://ffxivclassic.fragmenterworks.com/wiki/index.php/Game_Opcodes:Set_Actor_SubState
        public byte Breakage { get; set; } //Turns off and on the partsBreak flags for the model.Used to disable body parts as they are damaged (IE: Ifrit's Horn). Up to 8 flags.
        public byte Chant { get; set; } //Sets the casting animation for a model's left and right hands. HiNibble: Right hand, LoNibble: Left hand. You can check the bid file of a monster to find the animations. Also sets the state of the crafting ball.
        public byte ModBoolsGuard { get; set; } //Allocates a number of bits for bitfield use in the mode field. Doesn't seem to effect gameplay, rather used for debugging purposes (cmdDev).
        public byte Waste { get; set; } //Sets the guarding animation for a model's left or right hands. Used for when guarding was manual. Bit0: Right Hand, Bit1: Left Hand.
        public byte Mode { get; set; } //Dims the main weapon to signify damage. Unused in retail. Values go from 0-3, only HiNibble changes anything. Used to change the amount of arrows appearing in a quiver.
        public byte Unknown { get; set; } //Sets various passive graphical options for a model. Eg: Ifrit's Plumes, his glow, and HellFire. Can be a value or a bitfield, but the latter seems to be used 99% of the time.
        #endregion

        public ushort MotionPack { get; set; } //Sets the idle animation for this actor. Used by most event NPCs. Seems to only open "<model>/cmn/fid/<mpackID>" animations. Strangely the lua "get" method only returns a byte.
             
        public byte[] ToBytes()
        {
            byte[] data = new byte[0x08];

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(Breakage);
                    bw.Write(Chant);
                    bw.Write(ModBoolsGuard);
                    bw.Write(Waste);
                    bw.Write(Mode);
                    bw.Write(Unknown);
                    bw.Write(MotionPack);
                }
            }

            return data;
        }
    }
}
