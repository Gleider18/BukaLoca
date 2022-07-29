using System;
using System.Collections.Generic;
using UnityEngine;

namespace Databases
{
    [CreateAssetMenu(fileName = "Products Database", menuName = "ScriptableObjects/ProductsDatabase")]
    public class ProductsDatabase : ScriptableObject
    {
        [SerializeField] public List<Product> Products = new List<Product>();

        public Product GetProguctByName(string name)
        {
            foreach (var product in Products)
            {
                if (product.name == name)
                {
                    return product;
                }
            }
            throw new Exception("There is no product with name " + name);
        }
    }
}