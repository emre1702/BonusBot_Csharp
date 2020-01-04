using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace TDSAssembly
{
    partial class TDSModule
    {
        [Command("ConfirmTDS"), Alias("ConfirmIdentity", "ConfirmUserId")]
        public async Task ConfirmUserId()
        {
            try
            {
                var reply = await _connectorClient.UsedCommandAsync(new UsedCommandRequest
                {
                    UserId = Context.User.Id,
                    Command = "confirmtds"
                });

                if (!string.IsNullOrEmpty(reply.Message))
                    await ReplyAsync(reply.Message);
            }
            catch (Exception ex)
            {
                await ReplyAsync("Confirming failed:" + Environment.NewLine + ex.GetBaseException().Message);
            }
           
        }
    }
}
