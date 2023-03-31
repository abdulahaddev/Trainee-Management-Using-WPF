using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TMS.Models;

namespace TMS
{
    public partial class MainWindow : Window
    {
        public string jsonFile = "trainees.json";
        public FileInfo TempImageFile { get; set; }
        public BitmapImage DefaultImage => new BitmapImage(new Uri(ImageDirectory() + "default.png"));

        public MainWindow()
        {
            InitializeComponent();

            if (!File.Exists(jsonFile)) //Create Json File if not exists
            {
                File.CreateText(jsonFile).Close();
            }
            ShowData();

        }

        private bool IsValidJson(string data)   //check whether file contains json format or not
        {
            try
            {
                var temp = JObject.Parse(data);  //Try to parse json data if can't will throw exception
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void ShowData()
        {
            var json = File.ReadAllText(jsonFile);

            if (!IsValidJson(json))
            {
                return;
            }

            var jsonObj = JObject.Parse(json);
            var jsonList = jsonObj["Trainee"].ToString();
            var traineeList = JsonConvert.DeserializeObject<List<Trainee>>(jsonList);   //Deserialize to List<Employee>
            traineeList = traineeList.OrderBy(x => x.Id).ToList();                      //Sorting List<Employee> by Id (Ascending)

            foreach (var item in traineeList)
            {
                item.ImageSrc = ImageInstance(new Uri(ImageDirectory() + item.ImageTitle));   //Create image instance for all Employee
            }
            TraineeListView.ItemsSource = traineeList;
            TraineeListView.Items.Refresh();

            GC.Collect();                   //Call garbage collector to release unused image instance resource
            GC.WaitForPendingFinalizers();
        }
        public ImageSource ImageInstance(Uri path)  //Create image instance rather than referencing image file
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = path;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.DecodePixelWidth = 300;
            bitmap.EndInit();
            return bitmap;
        }
        public string ImageDirectory()    //Get the Image Directory Path Where Image is stored
        {
            var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            string assemblyDirectory = Path.GetDirectoryName(currentAssembly.Location);             // debug folder
            string ImagePath = Path.GetFullPath(Path.Combine(assemblyDirectory, @"..\..\Image\"));    // Navigate two levels up => Project directory
            return ImagePath;
        }

        private void List_Edit_Click(object sender, RoutedEventArgs e)
        {
            Button editButton = sender as Button;
            Trainee trainee = editButton.DataContext as Trainee;

            InsertWindow editWindow = new InsertWindow
            {
                State = "Update"
            };

            editWindow.txtId.Text = trainee.Id.ToString();
            editWindow.txtId.IsEnabled = false;
            editWindow.txtFirstName.Text = trainee.FirstName;
            editWindow.txtLastName.Text = trainee.LastName;
            editWindow.datePickerDob.SelectedDate = trainee.DateOfBirth;
            editWindow.txtEmail.Text = trainee.Email;
            editWindow.txtContact.Text = trainee.Contact;
            editWindow.cmbCourse.SelectedItem = trainee.Course;
            editWindow.BtnInsert.Content = "Update";
            editWindow.BtnUploadImage.Content = "Modify Image";
            editWindow.imageBox.Source = trainee.ImageSrc;

            if (trainee.Gender == "Male")
            {
                editWindow.rdMale.IsChecked = true;
            }
            else if (trainee.Gender == "Female")
            {
                editWindow.rdFemale.IsChecked = true;
            }

            if (trainee.Status == true)
            {
                editWindow.rdActive.IsChecked = true;
            }
            else if (trainee.Status == false)
            {
                editWindow.rdInactive.IsChecked = true;
            }
            this.Hide();
            editWindow.Show();

        }

        private void List_Update_Click(object sender, RoutedEventArgs e)
        {
            //Update update = new Update();
            //update.Owner = this;

            //Button b = sender as Button;
            //Trainee empbtn = b.CommandParameter as Trainee;

            //update.txtId.IsEnabled = false;
            //update.txtId.Text = empbtn.Id.ToString();
            //update.cmbTitle.Text = empbtn.Title;
            //update.txtFirstName.Text = empbtn.FirstName;
            //update.txtLastName.Text = empbtn.LastName;
            //update.txtEmail.Text = empbtn.Email;
            //update.txtContactNo.Text = empbtn.Contact;
            //update.ImgModify.Source = empbtn.ImageSrc;
            //this.Hide();
            //update.Show();
        }

        private void List_Delete_Click(object sender, RoutedEventArgs e)
        {
            var json = File.ReadAllText(jsonFile);
            var jsonObj = JObject.Parse(json);
            var jsonList = jsonObj["Trainee"].ToString();
            var traineeList = JsonConvert.DeserializeObject<List<Trainee>>(jsonList);   //Deserialize to List<Employee>

            Button btnDelete = sender as Button;
            Trainee trainee = btnDelete.DataContext as Trainee;

            MessageBoxResult result = MessageBox.Show($"Are you want to delete ID - {trainee.Id}", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes) //if press 'Yes' on delete confirmation
            {
                traineeList.Remove(traineeList.Find(x => x.Id == trainee.Id));   //Remove the employee from the list
                JArray empArray = JArray.FromObject(traineeList);       //Convert List<Trainee> to JArray
                jsonObj["Trainee"] = empArray;                    //Add JArray to 'Trainee' JProperty
                var newJsonResult = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

                FileInfo thisFile = new FileInfo(ImageDirectory() + trainee.ImageTitle);
                if (thisFile.Name != "default.png") //Delete image (Not default image)
                {
                    thisFile.Delete();
                }

                File.WriteAllText(jsonFile, newJsonResult);

                MessageBox.Show("Data Deleted Successfully !!", "Delete", MessageBoxButton.OK, MessageBoxImage.Question);
                ShowData();
            }
            else
            {
                return;
            }
        }

        private void BtnAddTrainee_Click(object sender, RoutedEventArgs e)
        {
            InsertWindow insertWindow = new InsertWindow
            {
                State = "Insert"
            };
            this.Hide();
            insertWindow.Show();

        }

        private void List_View_Click(object sender, RoutedEventArgs e)
        {
            Button viewButton = sender as Button;
            Trainee trainee = viewButton.DataContext as Trainee;

            PreviewWindow previewWindow = new PreviewWindow();
            previewWindow.imageBox.Source = trainee.ImageSrc;
            previewWindow.tbName.Text = trainee.FirstName + " " + trainee.LastName;
            previewWindow.tbId.Text = trainee.Id.ToString();
            previewWindow.tbGender.Text = trainee.Gender.ToString();
            previewWindow.tbBirthDate.Text = trainee.DateOfBirth.ToString("dd-MM-yyyy");
            previewWindow.tbEmail.Text = trainee.Email;
            previewWindow.tbContact.Text = trainee.Contact;
            previewWindow.tbStatus.Text = (trainee.Status == true) ? "Active" : "Inactive";

            this.Hide();
            previewWindow.Show();

        }
    }
}
