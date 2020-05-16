using System;
using System.Threading.Tasks;
using Discord.Commands;
using TDSConnectorClient;

namespace UtilityAssembly
{
    partial class UtilityModule
    {
        [Command("ConfirmTDS"), Alias("ConfirmIdentity", "ConfirmUserId")]
        public async Task ConfirmUserId()
        {
            try
            {
                var context = (SocketCommandContext)Context;
                var reply = await _tdsClient.UsedCommand(context.User.Id, "ConfirmTDS", null);

                await ReplyAsync(reply ?? "Command was sent");
            }
            catch (Exception ex)
            {
                await ReplyAsync("Confirming failed:" + Environment.NewLine + ex.GetBaseException().Message);
            }

        }
    }
}
