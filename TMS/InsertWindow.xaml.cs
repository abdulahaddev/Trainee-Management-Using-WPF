using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using TMS.Models;

namespace TMS
{
    /// <summary>
    /// Interaction logic for InsertWindow.xaml
    /// </summary>
    public partial class InsertWindow : Window
    {
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public string State { get; set; }

        public string filename = "trainees.json";
        public FileInfo TempImageFile { get; set; }
        public FileInfo OldImageFile { get; set; }          //Using for exists image
        public BitmapImage DefaultImage => new BitmapImage(new Uri(mainWindow.ImageDirectory() + "default.png"));
        public InsertWindow()
        {
            InitializeComponent();
            string[] courseList = new string[]
            {
                "C#",
                "J2EE",
                "Web Design",
                "Database"
            };
            imageBox.Source = (State == null || State == "Insert") ? DefaultImage : null;
            cmbCourse.ItemsSource = courseList;
            cmbCourse.SelectedItem = -1;

        }

        private void BtnUploadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog
            {
                Filter = "Image Files(*.jpg; *.jpeg; *.png;)|*.jpg; *.jpeg; *.png;",
                Title = "Select an Image"
            };
            if (fd.ShowDialog().Value == true)
            {
                imageBox.Source = mainWindow.ImageInstance(new Uri(fd.FileName));  //ImageInstance return new instance of image rather than image reference
                TempImageFile = new FileInfo(fd.FileName);
            }
        }

        private void BtnInsert_Click(object sender, RoutedEventArgs e)
        {
            if (State == "Insert")
            {
                Trainee trainee = new Trainee();
                try
                {
                    trainee.Id = int.Parse(txtId.Text);
                    trainee.FirstName = txtFirstName.Text;
                    trainee.LastName = txtLastName.Text;
                    trainee.Gender = (rdMale.IsChecked == true) ? "Male" : "Female";
                    trainee.DateOfBirth = datePickerDob.SelectedDate.Value;
                    trainee.Email = txtEmail.Text;
                    trainee.Contact = txtContact.Text;
                    trainee.Course = cmbCourse.SelectedItem.ToString();
                    trainee.Status = rdActive.IsChecked == true;
                    trainee.ImageTitle = (TempImageFile != null) ? $"{int.Parse(txtId.Text) + TempImageFile.Extension}" : "default.png";
                }
                catch (Exception)
                {
                    MessageBox.Show("Please Fill up all fields");
                    return;
                }

                string filedata = File.ReadAllText(filename);

                if (IsValidJson(filedata) && IsExists("Trainee") && IsIdExists(trainee.Id))
                {
                    MessageBox.Show($"ID - {trainee.Id} exists\nTry with different Id", "Message", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (IsValidJson(filedata) && IsExists("Trainee") && !IsIdExists(trainee.Id)) //check file contains valid json format and exists "Employees" Parent Node
                {

                    var jsonObj = JObject.Parse(filedata);
                    var jsonList = jsonObj["Trainee"].ToString();
                    var traineeList = JsonConvert.DeserializeObject<List<Trainee>>(jsonList);   //Deserialize to List<Trainee>

                    traineeList.Add(trainee);
                    JArray traineeArray = JArray.FromObject(traineeList);
                    jsonObj["Trainee"] = traineeArray;
                    var newJsonResult = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

                    if (TempImageFile != null)
                    {
                        TempImageFile.CopyTo(ImageDirectory() + trainee.ImageTitle);
                        TempImageFile = null;
                        imageBox.Source = DefaultImage;
                    }
                    File.WriteAllText(filename, newJsonResult);     //write all trainees to json file
                }

                if (!IsValidJson(filedata))
                {
                    var emp = new { Trainee = new Trainee[] { trainee } };  //create json format with parent[Trainee]
                    string newJsonResult = JsonConvert.SerializeObject(emp, Formatting.Indented);   //serialize json format
                    if (TempImageFile != null)
                    {
                        TempImageFile.CopyTo(ImageDirectory() + trainee.ImageTitle);
                        TempImageFile = null;
                        imageBox.Source = DefaultImage;
                    }
                    File.WriteAllText(filename, newJsonResult);         //write json format to trainees.json
                }
                this.Close();
                mainWindow.Show();
                mainWindow.ShowData();
            }
            if (State == "Update")
            {
                var Id = int.Parse(txtId.Text);
                var FirstName = txtFirstName.Text;
                var LastName = txtLastName.Text;
                var Gender = (rdMale.IsChecked == true) ? "Male" : "Female";
                var DateOfBirth = datePickerDob.SelectedDate.Value;
                var Email = txtEmail.Text;
                var Contact = txtContact.Text;
                var Course = cmbCourse.SelectedItem.ToString();
                var Status = rdActive.IsChecked == true;

                string filedata = File.ReadAllText(filename);
                var jsonObj = JObject.Parse(filedata);
                var jsonList = jsonObj["Trainee"].ToString();
                var traineeList = JsonConvert.DeserializeObject<List<Trainee>>(jsonList);   //Deserialize to List<Employee>

                foreach (var item in traineeList.Where(x => x.Id == Id))
                {
                    item.FirstName = FirstName;
                    item.LastName = LastName;
                    item.Gender = Gender;
                    item.DateOfBirth = DateOfBirth;
                    item.Email = Email;
                    item.Contact = Contact;
                    item.Course = Course;
                    item.Status = Status;
                    item.ImageTitle = (TempImageFile != null) ? $"{int.Parse(txtId.Text) + TempImageFile.Extension}" : item.ImageTitle;

                    OldImageFile = (item.ImageTitle != "default.png") ? new FileInfo(mainWindow.ImageDirectory() + item.ImageTitle) : null;   //ternary to evaluate null if exists image is default image

                    if (TempImageFile != null && OldImageFile == null)  //Check if upload image not null && exists image is null or default.png
                    {
                        TempImageFile.CopyTo(mainWindow.ImageDirectory() + item.Id + TempImageFile.Extension);
                        item.ImageTitle = item.Id + TempImageFile.Extension;
                        TempImageFile = null;
                    }
                    if (OldImageFile != null && TempImageFile != null) //Check if upload image not null && old image not null. Extra -> check if old file exists in directory
                    {
                        item.ImageTitle = item.Id + TempImageFile.Extension;
                        OldImageFile.Delete();      //Delete exists image
                        TempImageFile.CopyTo(mainWindow.ImageDirectory() + Id + TempImageFile.Extension); //Copy upload image to target directory
                        TempImageFile = null;
                    }

                }

                var empArray = JArray.FromObject(traineeList);  //Convert List<Trainee> to Jarray
                jsonObj["Trainee"] = empArray;            //Set Jarray to 'Trainee' JProperty
                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);  //Serialize data
                File.WriteAllText(filename, output);

                this.Close();
                mainWindow.ShowData();
                mainWindow.Show();
                MessageBox.Show("Data Updated Successfully !!");
            }

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
        private bool IsExists(string data)      //Check if exists parent node ('Employees') in json file
        {
            string filedata = File.ReadAllText(filename);

            var jsonObject = JObject.Parse(filedata);

            var exists = jsonObject[data];     //If not exists return null

            return (exists != null);
        }
        private bool IsIdExists(int inputId)    //input id from input box
        {
            string filedata = File.ReadAllText(filename);
            var jsonObj = JObject.Parse(filedata);
            var jsonList = jsonObj["Trainee"].ToString();
            var traineeList = JsonConvert.DeserializeObject<List<Trainee>>(jsonList);   //Deserialize to List<Employee>

            var exists = traineeList.Find(x => x.Id == inputId);                 //return employee if id found, else return null

            if (exists != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public string ImageDirectory()    //Get the Image Directory Path Where Image is stored
        {
            var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            string assemblyDirectory = Path.GetDirectoryName(currentAssembly.Location);             // debug folder
            string ImagePath = Path.GetFullPath(Path.Combine(assemblyDirectory, @"..\..\Image\"));    // Navigate two levels up => Project directory

            return ImagePath;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mainWindow.Show();
        }
    }
}
