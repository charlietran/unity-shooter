using System.Collections;
using System.Collections.Generic;
using System;

public class Utility {

    // Shuffles an array of any class using the Fisher-Yates algorithm
    public static T[] ShuffleArray<T>(T[] array, int seed) {
        System.Random pseudoRNG = new System.Random(seed);

        for (int i = 0; i < array.Length - 1; i++) {
            int randomIndex = pseudoRNG.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }

    public static string NumberToWords(int number) {
        if (number == 0)
            return "zero";

        if (number < 0)
            return "negative " + NumberToWords(Math.Abs(number));

        string words = "";

        if ((number / 1000000) > 0) {
            words += NumberToWords(number / 1000000) + " million ";
            number %= 1000000;
        }

        if ((number / 1000) > 0) {
            words += NumberToWords(number / 1000) + " thousand ";
            number %= 1000;
        }

        if ((number / 100) > 0) {
            words += NumberToWords(number / 100) + " hundred ";
            number %= 100;
        }

        if (number > 0) {
            if (words != "")
                words += "and ";

            var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            if (number < 20)
                words += unitsMap[number];
            else {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }
        }

        return words;
    }
}
