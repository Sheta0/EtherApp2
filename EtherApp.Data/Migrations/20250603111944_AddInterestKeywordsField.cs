using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtherApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInterestKeywordsField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "Interests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 1,
                column: "Keywords",
                value: "tech,computer,software,programming,code,developer,app,AI,artificial intelligence,digital,innovation,hardware");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 2,
                column: "Keywords",
                value: "science,research,study,experiment,lab,discovery,physics,biology,chemistry,astronomy,hypothesis,theory");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 3,
                column: "Keywords",
                value: "art,paint,drawing,design,creative,artist,sketch,canvas,gallery,exhibition,sculpture,illustration");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 4,
                column: "Keywords",
                value: "music,song,band,concert,album,guitar,piano,lyrics,melody,rhythm,instrument,performer,singer");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 5,
                column: "Keywords",
                value: "sport,team,game,play,athlete,fitness,exercise,competition,match,tournament,championship,league");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 6,
                column: "Keywords",
                value: "travel,trip,vacation,destination,journey,explore,tourism,sightseeing,adventure,backpacking,resort,hotel");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 7,
                column: "Keywords",
                value: "food,recipe,cook,bake,meal,restaurant,dish,cuisine,ingredient,flavor,culinary,chef,gastronomy");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 8,
                column: "Keywords",
                value: "fashion,style,clothes,outfit,trend,wear,designer,model,runway,collection,accessory,boutique");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 9,
                column: "Keywords",
                value: "game,gaming,player,console,play,level,videogame,boardgame,roleplaying,strategy,puzzle,esports");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 10,
                column: "Keywords",
                value: "book,read,author,novel,story,literature,fiction,nonfiction,biography,poetry,publish,chapter");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 11,
                column: "Keywords",
                value: "movie,film,cinema,actor,director,watch,scene,screenplay,Hollywood,blockbuster,indie,documentary");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 12,
                column: "Keywords",
                value: "health,wellness,medical,doctor,exercise,diet,nutrition,therapy,mindfulness,medicine,workout,vitality");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 13,
                column: "Keywords",
                value: "business,company,startup,entrepreneur,market,invest,finance,economy,strategy,management,leadership,innovation");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 14,
                column: "Keywords",
                value: "education,learn,teach,student,school,university,knowledge,academic,course,degree,professor,curriculum");

            migrationBuilder.UpdateData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: 15,
                column: "Keywords",
                value: "politics,government,policy,vote,election,law,democracy,debate,campaign,party,legislation,advocacy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "Interests");
        }
    }
}
