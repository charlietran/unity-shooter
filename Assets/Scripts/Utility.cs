using System.Collections;
using System.Collections.Generic;

public class Utility {

    // Shuffles an array of any class using the Fisher-Yates algorithm
    public static T[] ShuffleArray<T>(T[] array, int seed) {
        System.Random pseudoRNG = new System.Random(seed);

        for(int i = 0; i < array.Length - 1; i++) {
            int randomIndex = pseudoRNG.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }
}
