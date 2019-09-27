using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatAspnetCoreAuthentication.Data.Migrations
{
    public partial class ChatMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChatName",
                table: "ChatRooms",
                newName: "RoomName");

            migrationBuilder.RenameColumn(
                name: "ChatId",
                table: "ChatRooms",
                newName: "RoomId");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "ChatRooms",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "ChatMessages",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RoomId",
                table: "ChatMessages",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Time",
                table: "ChatMessages",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 128);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "ChatMessages");

            migrationBuilder.RenameColumn(
                name: "RoomName",
                table: "ChatRooms",
                newName: "ChatName");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "ChatRooms",
                newName: "ChatId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
