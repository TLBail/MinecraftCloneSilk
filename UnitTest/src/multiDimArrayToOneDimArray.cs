namespace UnitTest;

public class multiDimArrayToOneDimArray
{

    public const int ARRAY_SIZE = 16;
    
    public int getIndex(int x, int y, int z) {
        return (x * ARRAY_SIZE * ARRAY_SIZE ) + (y * ARRAY_SIZE) + z;
    }
    
    [Test]
    public void testArrayAreEquals() {
        int[,] array1 = new [,]{
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16   
        },
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16
        } };
        
        int[] array2 = array1.Cast<int>().ToArray();

        int index = 0;
        foreach (int valueArray1 in array1) {
            Assert.That(array2[index], Is.EqualTo(valueArray1));
            index++;
        }
    }

    [Test]
    public void getIndexIsWorking() {

        int[,,] array1 = new int[ARRAY_SIZE, ARRAY_SIZE, ARRAY_SIZE];
        for (int x = 0; x < ARRAY_SIZE; x++) {
            for (int y = 0; y < ARRAY_SIZE; y++) {
                for (int z = 0; z < ARRAY_SIZE; z++) {
                    array1[x, y, z] = x + y + z;
                }
            }
        }
        
        int[] array2 = array1.Cast<int>().ToArray();

        for (int x = 0; x < ARRAY_SIZE; x++) {
            for (int y = 0; y < ARRAY_SIZE; y++) {
                for (int z = 0; z < ARRAY_SIZE; z++) {
                    Assert.That(array1[x, y, z], Is.EqualTo(array2[getIndex(x,y,z)])); 
                }
            }
        }
        
    }
    
    
}