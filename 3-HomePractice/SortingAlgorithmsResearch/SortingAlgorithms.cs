namespace SortingAlgorithmsResearch;

public static class SortingAlgorithms
{
    public static void InsertionSort(double[] array)
    {
        for (int i = 1; i < array.Length; i++)
        {
            double key = array[i];
            int j = i - 1;

            while (j >= 0 && array[j] > key)
            {
                array[j + 1] = array[j];
                j--;
            }

            array[j + 1] = key;
        }
    }

    public static void SelectionSort(double[] array)
    {
        for (int i = 0; i < array.Length - 1; i++)
        {
            int minIndex = i;

            for (int j = i + 1; j < array.Length; j++)
            {
                if (array[j] < array[minIndex])
                    minIndex = j;
            }

            if (minIndex != i)
                (array[i], array[minIndex]) = (array[minIndex], array[i]);
        }
    }

    public static void MergeSort(double[] array)
    {
        if (array.Length < 2)
            return;

        double[] buffer = new double[array.Length];
        MergeSortRecursive(array, buffer, 0, array.Length - 1);
    }

    private static void MergeSortRecursive(double[] array, double[] buffer, int left, int right)
    {
        if (left >= right)
            return;

        int middle = left + (right - left) / 2;
        MergeSortRecursive(array, buffer, left, middle);
        MergeSortRecursive(array, buffer, middle + 1, right);
        Merge(array, buffer, left, middle, right);
    }

    private static void Merge(double[] array, double[] buffer, int left, int middle, int right)
    {
        int i = left;
        int j = middle + 1;
        int k = left;

        while (i <= middle && j <= right)
        {
            if (array[i] <= array[j])
                buffer[k++] = array[i++];
            else
                buffer[k++] = array[j++];
        }

        while (i <= middle)
            buffer[k++] = array[i++];

        while (j <= right)
            buffer[k++] = array[j++];

        for (int index = left; index <= right; index++)
            array[index] = buffer[index];
    }
}
