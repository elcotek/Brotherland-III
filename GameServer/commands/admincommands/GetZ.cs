// Author: Nydirac
// Date: 09/11/2016
//
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
    [CmdAttribute(
       "&getz",
       ePrivLevel.GM,
         "/getz *target* (self or other)")]
    public class getzCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            GameObject obj = null;



            if (client.Player != null)
            {
                if (client.Player.TargetObject != null)
                    obj = client.Player.TargetObject;
                else
                    obj = client.Player;
            }

            if (obj != null)
            {
                if (obj.CurrentZone.zActive)
                    client.Out.SendMessage(string.Format("Server calulated Z [{3}] for the Object {5} [X: {0} Y: {1}  Object's Z: {4}) ZoneID: {2}]", obj.X, obj.Y, obj.CurrentZone.ID, obj.CurrentZone.GetZ(obj.X, obj.Y), obj.Z, obj.ToString()), eChatType.CT_Help, eChatLoc.CL_SystemWindow);
                else
                    client.Out.SendMessage("Data for calculating Z not present!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
            }
        }
    }
}