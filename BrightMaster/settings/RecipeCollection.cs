using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster.Settings
{
    
    public class RecipeCollection : BindableBase
    {
        private ObservableCollection<Recipe> recipes = new ObservableCollection<Recipe>();
        private Recipe selectedRecipe;
        
        public RecipeCollection()
        {
            EnumRecipes();
        }

        private void EnumRecipes()
        {
            string recipeFolder = FolderHelper.GetRecipeFolder();
            DirectoryInfo dirInfo = new DirectoryInfo(recipeFolder);
            var files = dirInfo.EnumerateFiles("*.xml").ToList();
            foreach(var file in files)
            {
                Recipe recipe = new Recipe("dummy");
                SerializeHelper.LoadSettings(ref recipe, file.FullName);
                recipes.Add(recipe);
            }
            
            if (recipes.Count > 0)
                selectedRecipe = recipes[0];
        }

        public Recipe SelectedRecipe
        {
            get
            {
                return selectedRecipe;
            }
            set
            {
                SetProperty(ref selectedRecipe, value);
            }
        }

        public ObservableCollection<Recipe> Recipes
        {
            get
            {
                return recipes;
            }
            set
            {
                SetProperty(ref recipes, value);
            }
        }




        internal void AddNew()
        {
            string name = "";
            for(int i = 0; i< recipes.Count+1; i++)
            {
                name = string.Format("recipe{0}", i + 1);
                bool exist = IsExistName(name);
                if (!exist)
                    break;
            }
            Recipe newRecipe = new Recipe(name);
            recipes.Add(newRecipe);
            SelectedRecipe = newRecipe;
            
        }

        private bool IsExistName(string name)
        {
            foreach(var recipe in recipes)
            {
                if (recipe.Name == name)
                    return true;
            }
            return false;
        }

        internal void Remove()
        {
            if(selectedRecipe != null)
            {
                string tmpName = selectedRecipe.Name;
                recipes.Remove(selectedRecipe);
                if (recipes.Count > 0)
                {
                    string recipeFile = FolderHelper.GetRecipeFolder() + tmpName + ".xml";
                    File.Delete(recipeFile);
                    SelectedRecipe = recipes[recipes.Count - 1];
                }
                else
                    SelectedRecipe = null;
            }
        }
    }
}
