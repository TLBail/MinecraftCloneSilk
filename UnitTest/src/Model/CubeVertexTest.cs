using MinecraftCloneSilk.Model;

namespace UnitTest.Model;

[TestFixture]
[TestOf(typeof(CubeVertex))]
public class CubeVertexTest
{

    [Test]
    public void TestData() {
        CubeVertex vertex = new CubeVertex();
        vertex.SetAmbientOcclusion(3);
        Assert.That(vertex.GetAmbientOcclusion(), Is.EqualTo(3));
        
        vertex.SetLightLevel(15);
        Assert.That(vertex.GetLightLevel(), Is.EqualTo(15));
        Assert.That(vertex.GetAmbientOcclusion(), Is.EqualTo(3));
        
        
        vertex.SetAmbientOcclusion(2);
        Assert.That(vertex.GetAmbientOcclusion(), Is.EqualTo(2));
        Assert.That(vertex.GetLightLevel(), Is.EqualTo(15));
        
        vertex.SetLightLevel(0);
        Assert.That(vertex.GetLightLevel(), Is.EqualTo(0));
        Assert.That(vertex.GetAmbientOcclusion(), Is.EqualTo(2));
        
        
        
    }
}