﻿using AtlasGridDataLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerGridEditor
{
    public partial class CreateIslandForm : Form
    {
        public MainForm mainForm;
        public Island editedIsland;
        
        public bool bIslandNameChanged = false;
        public CreateIslandForm()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            foreach(TransientNodeTemplate transientNodeTemplate in mainForm.currentProject.transientNodeTemplates)
            {
                LandNodeTemplateComboBox.Items.Add(transientNodeTemplate.Key);
            }


            foreach (SpawnerInfoData spawnerInfo in mainForm.spawners.spawnersInfo)
                SpawnerTemplate.Items.Add((string)spawnerInfo.Name);

            foreach (FoliageAttachmentOverride foliageAttachmentOverride in mainForm.currentProject.foliageAttachmentOverrides)
                FoliageOverrideKey.Items.Add(foliageAttachmentOverride.Key);

            if (editedIsland != null)
            {
                bIslandNameChanged = false;
                islandNameTxtBox.Text = editedIsland.name;
                pictureBox1.ImageLocation = editedIsland.imagePath;
                sizeXTxtBox.Text = editedIsland.x + "";
                sizeYTxtBox.Text = editedIsland.y + "";
                landscapeMaterialOverrideTxtBox.Text = editedIsland.landscapeMaterialOverride + "";

                if (editedIsland.sublevelNames != null)
                    sublevelsList.Items.AddRange(editedIsland.sublevelNames.ToArray());

                this.Text = "Edit Island";
                createBtn.Text = "Edit";


                if (editedIsland.spawnerOverrides != null)
                    foreach (KeyValuePair<string, string> overrides in editedIsland.spawnerOverrides)
                    {
                        int index = spawnerOverridesGrid.Rows.Add();
                        spawnerOverridesGrid.Rows[index].Cells[SpawnerName.Name].Value = overrides.Key;
                        if (SpawnerTemplate.Items.Contains(overrides.Value))
                            spawnerOverridesGrid.Rows[index].Cells[SpawnerTemplate.Name].Value = overrides.Value;
                    }

                if (editedIsland.harvestOverrideKeys != null)
                    foreach (string harvestOverrideKey in editedIsland.harvestOverrideKeys)
                    {
                        int index = harvestOverridesGrid.Rows.Add();
                        harvestOverridesGrid.Rows[index].Cells[FoliageOverrideKey.Name].Value = harvestOverrideKey;
                    }

                minTreasureQualityTxtBox.Text = editedIsland.minTreasureQuality + "";
                maxTreasureQualityTxtBox.Text = editedIsland.maxTreasureQuality + "";

                useNpcVolumesForTreasuresChkBox.Checked = editedIsland.useNpcVolumesForTreasures;
                useLevelBoundsForTreasuresChkBox.Checked = editedIsland.useLevelBoundsForTreasures;
                prioritizeVolumesForTreasuresChkBox.Checked = editedIsland.prioritizeVolumesForTreasures;
                IslandTreasureBottleSupplyCrateOverridesTxtBox.Text = editedIsland.islandTreasureBottleSupplyCrateOverrides;
                isControlPointChkBox.Checked = editedIsland.isControlPoint;
                isControlPointAllowCaptureChckBox.Checked = editedIsland.isControlPointAllowCapture;

                islandPointsTxtBox.Text = editedIsland.islandPoints + "";
                singleSpawnPointXTxtBox.Text = editedIsland.singleSpawnPointX + "";
                singleSpawnPointYTxtBox.Text = editedIsland.singleSpawnPointY + "";
                singleSpawnPointZTxtBox.Text = editedIsland.singleSpawnPointZ + "";
                textBox1.Text = editedIsland.maxIslandClaimFlagZ + "";
                

                if (editedIsland.extraSublevels != null)
                    extraSublevelsTxtBox.Lines = editedIsland.extraSublevels.ToArray();

                if (editedIsland.treasureMapSpawnPoints != null)
                    TreasureMapSpawnPointsTxtBox.Lines = editedIsland.treasureMapSpawnPoints.ToArray();

                if (editedIsland.wildPirateCampSpawnPoints != null)
                    WildPirateCampSpawnPointsTxtBox.Lines = editedIsland.wildPirateCampSpawnPoints.ToArray();

                foreach (IslandInstanceData islandInstance in mainForm.currentProject.islandInstances)
                    if(islandInstance.name == editedIsland.name)
                    {
                        Server s = islandInstance.GetCurrentServer(mainForm);
                        if (s != null)
                            instancesListBox.Items.Add(string.Format("({0}, {1})", s.gridX, s.gridY));
                    }

                modNameTxtBox.Text = editedIsland.modDir;

                foreach (TransientNodeTemplate transientNodeTemplate in mainForm.currentProject.transientNodeTemplates)
                {
                    if(transientNodeTemplate.Key == editedIsland.landNodeKey)
                    {
                        LandNodeTemplateComboBox.SelectedItem = transientNodeTemplate.Key;
                    }
                }
            }
        }

        private void chooseImgBtn_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "png files (*.png)|*.png";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                pictureBox1.ImageLocation = fileName;
            }
        }

        private void sizeXTxtBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            StaticHelpers.ForceNumericKeypress(sender, e);
        }

        private void sizeYTxtBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            StaticHelpers.ForceNumericKeypress(sender, e);
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void createBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(islandNameTxtBox.Text))
            {
                MessageBox.Show("Invalid island name", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            float x, y;
            if (!float.TryParse(sizeXTxtBox.Text, out x) || !float.TryParse(sizeYTxtBox.Text, out y))
            {
                MessageBox.Show("Invalid island dimensions", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(pictureBox1.ImageLocation))
            {
                MessageBox.Show("Invalid image", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            int landscapeMaterialOverride = -1;
            if (!int.TryParse(landscapeMaterialOverrideTxtBox.Text, out landscapeMaterialOverride))
            {
                MessageBox.Show("Invalid landscape material override index", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Make sure there are no duplicate names
            HashSet<string> names = new HashSet<string>();
            foreach (DataGridViewRow row in spawnerOverridesGrid.Rows)
            {
                if (row.Index == spawnerOverridesGrid.Rows.Count - 1) continue; //Last row is the new row

                string name = (string)row.Cells[SpawnerName.Name].Value;

                if (names.Contains(name))
                {
                    //Duplicate name
                    MessageBox.Show("Duplicate spawner override names found\nOverride names must be unique", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                names.Add(name);
            }

            foreach (DataGridViewRow row in spawnerOverridesGrid.Rows)
            {
                if (row.Index == spawnerOverridesGrid.Rows.Count - 1) continue; //Last row is the new row

                string val = (string)row.Cells[SpawnerTemplate.Name].Value;
                if (string.IsNullOrEmpty(val))
                {
                    //invalid template
                    MessageBox.Show(string.Format("Template not selected for {0}", (string)row.Cells[SpawnerName.Name].Value), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }


            names = new HashSet<string>();
            foreach (DataGridViewRow row in harvestOverridesGrid.Rows)
            {
                if (row.Index == harvestOverridesGrid.Rows.Count - 1) continue; //Last row is the new row
                string name = (string)row.Cells[FoliageOverrideKey.Name].Value;
                if (names.Contains(name))
                {
                    //Duplicate name
                    //MessageBox.Show("Duplicate harvest override names found\nOverride names must be unique", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }
                names.Add(name);
            }

            float minTreasureQuality = -1;
            float maxTreasureQuality = -1;

            float.TryParse(minTreasureQualityTxtBox.Text, out minTreasureQuality);
            float.TryParse(maxTreasureQualityTxtBox.Text, out maxTreasureQuality);

            float spSpawnPointX = 0.0f;
            float spSpawnPointY = 0.0f;
            float spSpawnPointZ = 0.0f;

            float.TryParse(singleSpawnPointXTxtBox.Text, out spSpawnPointX);
            float.TryParse(singleSpawnPointYTxtBox.Text, out spSpawnPointY);
            float.TryParse(singleSpawnPointZTxtBox.Text, out spSpawnPointZ);

            float maxIslandZ = 0.0f;
            float.TryParse(textBox1.Text, out maxIslandZ);

            int islandPoints = 1;
            int.TryParse(islandPointsTxtBox.Text, out islandPoints);
            string islandRemovedFromMod = null;
            if (editedIsland != null)
            {
                if (islandNameTxtBox.Text != editedIsland.name) //name changed
                {
                    if (mainForm.islands.ContainsKey(islandNameTxtBox.Text))
                    {
                        MessageBox.Show("An island with the same name already exist.", "Error", MessageBoxButtons.OK);
                        return;
                    }

                    if (MessageBox.Show("Renaming islands will result in renaming all placed islands in the opened project.\nNote: The editor will not be able to load projects that contained the old name.\nSave?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                        return;

                    //rename all the instances in the current project
                    if (mainForm.currentProject != null)
                    {
                        foreach (IslandInstanceData instance in mainForm.currentProject.islandInstances)
                        {
                            if (instance.name == editedIsland.name)
                                instance.name = islandNameTxtBox.Text;
                        }
                    }

                    mainForm.islands.Remove(editedIsland.name);
                    if (islandNameTxtBox.Text != editedIsland.name)
                    {
                        bIslandNameChanged = true;
                    }
                    editedIsland.name = islandNameTxtBox.Text;
                    mainForm.islands.Add(editedIsland.name, editedIsland);

                    //rename image
                    string originalDirectory = Path.GetDirectoryName(editedIsland.imagePath);
                    string newImgPath = originalDirectory + "/" + editedIsland.name + "_img.png";
                    editedIsland.InvalidateImage();
                    File.Move(editedIsland.imagePath, newImgPath);

                    if (pictureBox1.ImageLocation == editedIsland.imagePath)
                        pictureBox1.ImageLocation = newImgPath;

                    editedIsland.imagePath = newImgPath;
                }

                if (editedIsland.modDir != null && editedIsland.modDir != null && modNameTxtBox.Text != editedIsland.modDir)
                {
                    //Mod dir changed
                    if (!string.IsNullOrWhiteSpace(modNameTxtBox.Text))
                    {
                        modNameTxtBox.Text.Trim();

                        string modDir = null;
                        modDir = MainForm.islandModsDir + "/" + modNameTxtBox.Text;

                        if (!Directory.Exists(modDir))
                            Directory.CreateDirectory(modDir);
                        if (!Directory.Exists(modDir + MainForm.modImgsDir))
                            Directory.CreateDirectory(modDir + MainForm.modImgsDir);

                        editedIsland.InvalidateImage();
                        
                        string newImgPath = modDir + MainForm.modImgsDir + editedIsland.name + "_img.png";
                        File.Move(editedIsland.imagePath, newImgPath);

                        islandRemovedFromMod = editedIsland.modDir;
                        editedIsland.modDir = modNameTxtBox.Text;

                        if (pictureBox1.ImageLocation == editedIsland.imagePath)
                            pictureBox1.ImageLocation = newImgPath;

                        editedIsland.imagePath = newImgPath;
                    }
                    else
                    {
                        editedIsland.InvalidateImage();
                        string newImgPath = MainForm.imgsDir + "/" + editedIsland.name + "_img.png";
                        if (File.Exists(newImgPath))
                            File.Delete(newImgPath);
                        File.Move(editedIsland.imagePath, newImgPath);

                        islandRemovedFromMod = editedIsland.modDir;
                        editedIsland.modDir = null;

                        if (pictureBox1.ImageLocation == editedIsland.imagePath)
                            pictureBox1.ImageLocation = newImgPath;

                        editedIsland.imagePath = newImgPath;
                    }
                }

                editedIsland.x = x;
                editedIsland.y = y;

                if (pictureBox1.ImageLocation != editedIsland.imagePath) //picture changed
                {
                    editedIsland.InvalidateImage();
                    File.Copy(pictureBox1.ImageLocation, editedIsland.imagePath, true);
                }

                editedIsland.landscapeMaterialOverride = landscapeMaterialOverride;

                editedIsland.sublevelNames = new List<string>(sublevelsList.Items.Cast<string>());


                editedIsland.spawnerOverrides = new Dictionary<string, string>();

                foreach (DataGridViewRow row in spawnerOverridesGrid.Rows)
                {
                    if (row.Index == spawnerOverridesGrid.Rows.Count - 1) continue; //Last row is the new row

                    string name = (string)row.Cells[SpawnerName.Name].Value;
                    string template = (string)row.Cells[SpawnerTemplate.Name].Value;

                    editedIsland.spawnerOverrides.Add(name, template);
                }

                if (!DontSaveHarvestOverrides)
                {
                    editedIsland.harvestOverrideKeys = new List<string>();

                    foreach (DataGridViewRow row in harvestOverridesGrid.Rows)
                    {
                        if (row.Index == harvestOverridesGrid.Rows.Count - 1) continue; //Last row is the new row

                        string foliageOverrideKey = (string)row.Cells[FoliageOverrideKey.Name].Value;

                        if (!editedIsland.harvestOverrideKeys.Contains(foliageOverrideKey))
                            editedIsland.harvestOverrideKeys.Add(foliageOverrideKey);
                    }
                }

                editedIsland.minTreasureQuality = minTreasureQuality;
                editedIsland.maxTreasureQuality = maxTreasureQuality;

                editedIsland.useNpcVolumesForTreasures = useNpcVolumesForTreasuresChkBox.Checked;
                editedIsland.useLevelBoundsForTreasures = useLevelBoundsForTreasuresChkBox.Checked;
                editedIsland.prioritizeVolumesForTreasures = prioritizeVolumesForTreasuresChkBox.Checked;
                editedIsland.isControlPoint = isControlPointChkBox.Checked;
                editedIsland.isControlPointAllowCapture = isControlPointAllowCaptureChckBox.Checked;
                editedIsland.singleSpawnPointX = spSpawnPointX;
                editedIsland.singleSpawnPointY = spSpawnPointY;
                editedIsland.singleSpawnPointZ = spSpawnPointZ;
                editedIsland.maxIslandClaimFlagZ = maxIslandZ;
                if (LandNodeTemplateComboBox.SelectedItem != null)
                    editedIsland.landNodeKey = LandNodeTemplateComboBox.SelectedItem.ToString();
                else
                    editedIsland.landNodeKey = "";
                editedIsland.islandTreasureBottleSupplyCrateOverrides = IslandTreasureBottleSupplyCrateOverridesTxtBox.Text;

                editedIsland.islandPoints = islandPoints;
                List<string> NewEntries = new List<string>(extraSublevelsTxtBox.Lines);
                NewEntries.RemoveAll(item => { return string.IsNullOrWhiteSpace(item); });
                editedIsland.extraSublevels = NewEntries;

                List<string> NewTreasureMapEntries = new List<string>(TreasureMapSpawnPointsTxtBox.Lines);
                NewTreasureMapEntries.RemoveAll(item => { return string.IsNullOrWhiteSpace(item); });
                if (NewTreasureMapEntries.Count == 0)
                    NewTreasureMapEntries = null;
                editedIsland.treasureMapSpawnPoints = NewTreasureMapEntries;

                List<string> NewWildPirateCampEntries = new List<string>(WildPirateCampSpawnPointsTxtBox.Lines);
                NewWildPirateCampEntries.RemoveAll(item => { return string.IsNullOrWhiteSpace(item); });
                if (NewWildPirateCampEntries.Count == 0)
                    NewWildPirateCampEntries = null;
                editedIsland.wildPirateCampSpawnPoints = NewWildPirateCampEntries;
                

                mainForm.SaveIslands(islandRemovedFromMod);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                string Name = islandNameTxtBox.Text;

                if (mainForm.islands.ContainsKey(Name))
                {
                    MessageBox.Show("Duplicate island name", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string ImgLocation = pictureBox1.ImageLocation;
                List<string> sublevelNames = new List<string>(sublevelsList.Items.Cast<string>());

                List<string> treasureMapSpawnPoints = new List<string>(TreasureMapSpawnPointsTxtBox.Lines);
                treasureMapSpawnPoints.RemoveAll(item => { return string.IsNullOrWhiteSpace(item); });
                if (treasureMapSpawnPoints.Count == 0)
                    treasureMapSpawnPoints = null;

                List<string> wildPirateCampSpawnPoints = new List<string>(WildPirateCampSpawnPointsTxtBox.Lines);
                wildPirateCampSpawnPoints.RemoveAll(item => { return string.IsNullOrWhiteSpace(item); });
                if (wildPirateCampSpawnPoints.Count == 0)
                    wildPirateCampSpawnPoints = null;

                Dictionary<string, string> spawnerOverrides = new Dictionary<string, string>();

                foreach (DataGridViewRow row in spawnerOverridesGrid.Rows)
                {
                    if (row.Index == spawnerOverridesGrid.Rows.Count - 1) continue; //Last row is the new row

                    string name = (string)row.Cells[SpawnerName.Name].Value;
                    string template = (string)row.Cells[SpawnerTemplate.Name].Value;

                    spawnerOverrides.Add(name, template);
                }

                List<string> harvestOverrideKeys = new List<string>();

                foreach (DataGridViewRow row in harvestOverridesGrid.Rows)
                {
                    if (row.Index == harvestOverridesGrid.Rows.Count - 1) continue; //Last row is the new row

                    string foliageOverrideKey = (string)row.Cells[FoliageOverrideKey.Name].Value;

                    harvestOverrideKeys.Add(foliageOverrideKey);
                }

                string modDir = null;
                if (!string.IsNullOrWhiteSpace(modNameTxtBox.Text))
                {
                    modDir = MainForm.islandModsDir + "/" + modNameTxtBox.Text;
                    if (!Directory.Exists(modDir))
                        Directory.CreateDirectory(modDir);
                    if(!Directory.Exists(modDir + MainForm.modImgsDir))
                        Directory.CreateDirectory(modDir + MainForm.modImgsDir);
                }

                //Copy the image to our local imgs directory
                string newImgPath = (modDir != null) ? (modDir + MainForm.modImgsDir) : MainForm.imgsDir;
                newImgPath += "/" + Name + "_img.png";
                File.Copy(ImgLocation, newImgPath, true);


                mainForm.islands.Add(Name, new Island(Name, x, y, newImgPath, landscapeMaterialOverride, sublevelNames, spawnerOverrides, harvestOverrideKeys,
                    treasureMapSpawnPoints, wildPirateCampSpawnPoints, minTreasureQuality, maxTreasureQuality, useNpcVolumesForTreasuresChkBox.Checked, useLevelBoundsForTreasuresChkBox.Checked, 
                    prioritizeVolumesForTreasuresChkBox.Checked, isControlPointChkBox.Checked, isControlPointAllowCaptureChckBox.Checked, IslandTreasureBottleSupplyCrateOverridesTxtBox.Text, LandNodeTemplateComboBox.Text, new List<string>(extraSublevelsTxtBox.Lines), islandPoints, spSpawnPointX, spSpawnPointY, spSpawnPointZ, maxIslandZ));

                mainForm.islands.Last().Value.modDir = modNameTxtBox.Text.Trim();

                mainForm.RefreshIslandList();
                mainForm.SaveIslands();

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void addSublevels_Click(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory = Path.GetFullPath(GlobalSettings.Instance.GameSeamlessMapsDir);//mainForm.editorConfig.LastMapsFolder;
            //if (string.IsNullOrEmpty(openFileDialog.InitialDirectory) || !Directory.Exists(openFileDialog.InitialDirectory))
            //{
            //    //revert back to the maps folder defined
            //    openFileDialog.InitialDirectory = Path.GetFullPath(MainForm.gameMapsRelativePath);
            //}

            openFileDialog.Multiselect = true;

            openFileDialog.Filter = "umap files (*.umap)|*.umap";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //mainForm.editorConfig.LastMapsFolder = Path.GetDirectoryName(openFileDialog.FileName);
                mainForm.SaveConfig();

                foreach (string fileName in openFileDialog.FileNames)
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    if (!sublevelsList.Items.Contains(nameWithoutExt))
                        sublevelsList.Items.Add(nameWithoutExt);
                }
            }
        }

        private void sublevelsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                List<string> selectedItems = new List<string>(sublevelsList.SelectedItems.Cast<string>());

                foreach (string item in selectedItems)
                    sublevelsList.Items.Remove(item);
            }
        }

        private void ImportHarvestOverridesButton_Click(object sender, EventArgs e)
        {
            /*openFileDialog.Filter = "csv files (*.csv)|*.csv";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                StreamReader CSVReader = new StreamReader(openFileDialog.FileName);
                if (CSVReader != null && !CSVReader.EndOfStream)
                {
                    harvestOverridesGrid.Rows.Clear();
                    do
                    {
                        String Line = CSVReader.ReadLine();
                        if (Line != null && Line.Length > 0)
                        {
                            String[] Values = Line.Split(',');
                            if (Values.Length >= 2)
                            {
                                int index = harvestOverridesGrid.Rows.Add();
                                harvestOverridesGrid.Rows[index].Cells[FoliageTypeName.Name].Value = Values[0];
                                harvestOverridesGrid.Rows[index].Cells[OverrideActorComponentName.Name].Value = Values[1];
                            }
                        }
                    }
                    while (!CSVReader.EndOfStream);
                }

            }*/
        }

        private void ExportHarvestOverridesButton_Click(object sender, EventArgs e)
        {
            /*saveFileDialog.Filter = "csv files (*.csv)|*.csv";
            string ExportPath = Path.GetFullPath(GlobalSettings.Instance.ExportDir);
            if (!Directory.Exists(ExportPath))
                Directory.CreateDirectory(ExportPath);
            saveFileDialog.InitialDirectory = ExportPath;
            saveFileDialog.FileName = "harvestoverrrides.csv";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                StreamWriter CSVWriter = new StreamWriter(saveFileDialog.FileName);
                if (CSVWriter != null)
                {
                    foreach (DataGridViewRow Row in harvestOverridesGrid.Rows)
                    {
                        if (Row.Index == harvestOverridesGrid.Rows.Count - 1) continue; //Last row is the new row
                        CSVWriter.WriteLine(Row.Cells[FoliageTypeName.Name].Value + "," + Row.Cells[OverrideActorComponentName.Name].Value);
                    }
                    CSVWriter.Close();
                }
            }*/
        }

        private void harvestOverridesGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        bool DontSaveHarvestOverrides = false;

        private void harvestOverridesGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            DontSaveHarvestOverrides = true;
            harvestOverridesGrid.Enabled = false;

            label21.Text = "Harvest Overrides (disabled: none in project)";
        }
    }
}
