using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoHost.Server.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_AspNetUsers_SubscribedToID",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_AspNetUsers_SubscriberID",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "SubscriberID",
                table: "Subscriptions",
                newName: "SubscriberId");

            migrationBuilder.RenameColumn(
                name: "SubscribedToID",
                table: "Subscriptions",
                newName: "SubscribedToId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_SubscriberID",
                table: "Subscriptions",
                newName: "IX_Subscriptions_SubscriberId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_SubscribedToID",
                table: "Subscriptions",
                newName: "IX_Subscriptions_SubscribedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_AspNetUsers_SubscribedToId",
                table: "Subscriptions",
                column: "SubscribedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_AspNetUsers_SubscriberId",
                table: "Subscriptions",
                column: "SubscriberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_AspNetUsers_SubscribedToId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_AspNetUsers_SubscriberId",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "SubscriberId",
                table: "Subscriptions",
                newName: "SubscriberID");

            migrationBuilder.RenameColumn(
                name: "SubscribedToId",
                table: "Subscriptions",
                newName: "SubscribedToID");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_SubscriberId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_SubscriberID");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_SubscribedToId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_SubscribedToID");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_AspNetUsers_SubscribedToID",
                table: "Subscriptions",
                column: "SubscribedToID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_AspNetUsers_SubscriberID",
                table: "Subscriptions",
                column: "SubscriberID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
