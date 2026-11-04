using Gruppi.Services;
using Xunit;

public class DimensioneServiceTests
{
    private static (GruppoService g, DimensioneService d, FakeGruppoRepository repo) Make()
    {
        var repo = new FakeGruppoRepository();
        repo.Sessioni.Add("SES1");
        repo.DocentiPerSessione["SES1"] = "prof@unibo.it";
        repo.Studenti.UnionWith(new[] { "capo@studio.it", "x@studio.it" });
        return (new GruppoService(repo), new DimensioneService(repo), repo);
    }

    [Fact]
    public async Task Capogruppo_Can_Request_Size_Change()
    {
        var (g, d, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        var dto = await d.RequestAsync("capo@studio.it", grp.IdGruppo, new(4, "ci serve un quarto membro"));
        Assert.NotNull(dto);
        Assert.Equal("SES1", dto!.IdSessione);
        Assert.Contains("nuovaDimensione=4", dto.Descrizione);
    }

    [Fact]
    public async Task Non_Capogruppo_Cannot_Request()
    {
        var (g, d, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        var dto = await d.RequestAsync("x@studio.it", grp.IdGruppo, new(4, "no"));
        Assert.Null(dto);
    }

    [Fact]
    public async Task List_Returns_Group_Requests()
    {
        var (g, d, repo) = Make();
        var grp = await g.CreateAsync("capo@studio.it", new("SES1"));
        await d.RequestAsync("capo@studio.it", grp.IdGruppo, new(4, "uno"));
        var list = await d.ListPerGruppoAsync("capo@studio.it", grp.IdGruppo);
        Assert.Single(list);
    }
}
