﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XenoKit.Editor;
using XenoKit.Engine.Animation;
using XenoKit.Engine.Model;
using XenoKit.Engine.Shader;
using XenoKit.Engine.Textures;
using Xv2CoreLib;
using Xv2CoreLib.BCS;
using Xv2CoreLib.EAN;
using Xv2CoreLib.EMB_CLASS;
using Xv2CoreLib.EMD;
using Xv2CoreLib.EMM;
using Xv2CoreLib.ESK;

namespace XenoKit.Engine
{
    public class CharaPartSet : Entity
    {
        //PartSet (base)
        public int ID;
        private PartSet partSet = null;

        private Actor chara;

        public EAN_File FceEan = null;
        public EAN_File FceForeheadEan = null;

        public CharaPart[] Parts = new CharaPart[10];
        public CharaPart[] TransformedParts = new CharaPart[10];

        //BAC PartSet Function:
        public CharaPart[] BacTransformedParts = new CharaPart[10];
        public bool[] BacPermanentParts = new bool[10];

        public PartTypeFlags HideFlags = 0;
        public PartTypeFlags HideMatFlags = 0;

        //Custom Colors
        public List<CustomColorGroup> BcsColors = new List<CustomColorGroup>();

        public CharaPartSet(GameBase gameBase, Actor chara, int partSetId) : base(gameBase)
        {
            GameBase = gameBase;
            ID = partSetId;
            this.chara = chara;
            LoadPartSet();
        }

        #region LoadBase
        public void LoadPartSet()
        {
            partSet = chara.CharacterData.BcsFile.File.PartSets.FirstOrDefault(x => x.ID == ID);
            if (partSet == null)
            {
                Log.Add($"CharaPartSet.LoadPartSet: CharaPartSet is no longer valid, cant load.", LogType.Error);
                return;
            }

            ResetParts();

            //Load parts. Null parts will be ignored.
            LoadPart(0, false, false);
            LoadPart(1, false, false);
            LoadPart(2, false, false);
            LoadPart(3, false, false);
            LoadPart(4, false, false);
            LoadPart(5, false, false);
            LoadPart(6, false, false);
            LoadPart(7, false, false);
            LoadPart(8, false, false);
            LoadPart(9, false, false);

            //Apply BCS colors after all parts have been loaded
            SetBcsColors(partSet);
            ApplyCustomColors();

            //Load flags
            LoadHideFlags();
            LoadHideMatFlags();

            //Load fce.ean
            GetFceEan();

            //Create hitbox for character
            GetAABB();
            
        }

        public void LoadPart(int index, bool fetchPartSet = true, bool loadFlags = true)
        {
            if (fetchPartSet)
                partSet = chara.CharacterData.BcsFile.File.PartSets.FirstOrDefault(x => x.ID == ID);

            if (partSet == null)
            {
                Log.Add($"CharaPartSet.LoadPart: CharaPartSet is no longer valid, cant load.", LogType.Error);
                return;
            }

            Part part = partSet.GetPart(index);

            //Part was null. 
            if (part == null)
            {
                Parts[index] = null;
                return;
            }

            Parts[index] = new CharaPart(GameBase, chara, part, (PartType)index);

            if (loadFlags)
            {
                LoadHideFlags();
                LoadHideMatFlags();
            }

            //Reload fce.ean
            GetFceEan();
        }

        private void ResetParts()
        {
            for (int i = 0; i < Parts.Length; i++)
                Parts[i] = null;

            //Reload fce.ean
            GetFceEan();
        }

        public void LoadHideFlags()
        {
            PartTypeFlags flags = 0;

            for (int i = 0; i < Parts.Length; i++)
            {
                if (Parts[i] != null)
                {
                    flags |= Parts[i].GetHideFlags();
                }
            }

            for (int i = 0; i < TransformedParts.Length; i++)
            {
                if (Parts[i] != null)
                {
                    flags |= Parts[i].GetHideFlags();
                }
            }

            for (int i = 0; i < BacTransformedParts.Length; i++)
            {
                if (BacTransformedParts[i] != null)
                {
                    flags |= BacTransformedParts[i].GetHideFlags();
                }
            }

            HideFlags = flags;
        }

        public void LoadHideMatFlags()
        {
            PartTypeFlags flags = 0;

            for (int i = 0; i < Parts.Length; i++)
            {
                if (Parts[i] != null)
                {
                    flags |= Parts[i].GetHideMatFlags();
                }
            }

            for (int i = 0; i < TransformedParts.Length; i++)
            {
                if (Parts[i] != null)
                {
                    flags |= Parts[i].GetHideMatFlags();
                }
            }

            for (int i = 0; i < BacTransformedParts.Length; i++)
            {
                if (BacTransformedParts[i] != null)
                {
                    flags |= BacTransformedParts[i].GetHideMatFlags();
                }
            }

            HideMatFlags = flags;
        }

        private void GetFceEan()
        {
            FceForeheadEan = null;
            FceEan = null;

            //Look on base parts first
            for (int i = 0; i < Parts.Length; i++)
            {
                if (Parts[i] != null)
                {
                    if (Parts[i].GetEan() != null)
                    {
                        if (Parts[i].partType == PartType.FaceForehead)
                        {
                            FceForeheadEan = Parts[i].GetEan();
                        }
                        else if (Parts[i].partType == PartType.FaceBase)
                        {
                            FceEan = Parts[i].GetEan();
                        }
                    }
                }
            }

            //Transformed parts have priority, so they come last
            for (int i = 0; i < TransformedParts.Length; i++)
            {
                if (TransformedParts[i] != null)
                {
                    if (TransformedParts[i].GetEan() != null)
                    {
                        if (TransformedParts[i].partType == PartType.FaceForehead)
                        {
                            FceForeheadEan = TransformedParts[i].GetEan();
                        }
                        else if (TransformedParts[i].partType == PartType.FaceBase)
                        {
                            FceEan = TransformedParts[i].GetEan();
                        }
                    }
                }
            }

            //Transformed parts have priority, so they come last
            for (int i = 0; i < BacTransformedParts.Length; i++)
            {
                if (BacTransformedParts[i] != null)
                {
                    if (BacTransformedParts[i].GetEan() != null)
                    {
                        if (BacTransformedParts[i].partType == PartType.FaceForehead)
                        {
                            FceForeheadEan = BacTransformedParts[i].GetEan();
                        }
                        else if (BacTransformedParts[i].partType == PartType.FaceBase)
                        {
                            FceEan = BacTransformedParts[i].GetEan();
                        }
                    }
                }
            }
        }
        #endregion

        #region LoadTransformation
        public void ApplyTransformation(int index, PartTypeFlags partFlags = PartTypeFlags.AllParts, bool permanent = false)
        {
            if (index == -1) return;

            PartSet partSetTrans = chara.CharacterData.BcsFile.File.PartSets.FirstOrDefault(x => x.ID == index);
            if (partSetTrans == null)
            {
                Log.Add($"CharaPartSet.ApplyTransformation: PartSet {index} not found.", LogType.Warning);
                return;
            }

            //Load parts
            for (int i = 0; i < 10; i++)
            {
                if (partFlags.HasFlag(CharaPart.GetPartTypeAsFlag((PartType)i)))
                {
                    ApplyTransformedPart(i, partSetTrans, permanent);
                }
            }

            //Apply BCS colors after all parts have been loaded
            ApplyCustomColors();

            //Reload fce.ean
            GetFceEan();
        }

        private void ApplyTransformedPart(int index, PartSet partSet, bool permanent)
        {
            Part part = partSet.GetPart(index);

            if (part != null)
            {
                SetBcsColors(part);

                if (permanent)
                {
                    Parts[index] = new CharaPart(GameBase, chara, part, (PartType)index);
                    TransformedParts[index] = null;
                }
                else
                {
                    TransformedParts[index] = new CharaPart(GameBase, chara, part, (PartType)index);
                }
            }
        }

        public void ResetTransformation()
        {
            for (int i = 0; i < TransformedParts.Length; i++)
                TransformedParts[i] = null;

            //Reload fce.ean
            GetFceEan();
        }
        #endregion

        #region BacPartSetSwap
        public void ApplyBacPartSetSwap(int index, bool permanent)
        {
            var partSetTrans = chara.CharacterData.BcsFile.File.PartSets.FirstOrDefault(x => x.ID == index);
            if (partSetTrans == null)
            {
                Log.Add($"CharaPartSet.ApplyBacTransformation: PartSet {index} not found.", LogType.Warning);
                return;
            }

            //Load parts
            ApplyBacPartSetSwap(0, partSetTrans, permanent);
            ApplyBacPartSetSwap(1, partSetTrans, permanent);
            ApplyBacPartSetSwap(2, partSetTrans, permanent);
            ApplyBacPartSetSwap(3, partSetTrans, permanent);
            ApplyBacPartSetSwap(4, partSetTrans, permanent);
            ApplyBacPartSetSwap(5, partSetTrans, permanent);
            ApplyBacPartSetSwap(6, partSetTrans, permanent);
            ApplyBacPartSetSwap(7, partSetTrans, permanent);
            ApplyBacPartSetSwap(8, partSetTrans, permanent);
            ApplyBacPartSetSwap(9, partSetTrans, permanent);

            //Reload fce.ean
            GetFceEan();
        }

        private void ApplyBacPartSetSwap(int index, PartSet partSet, bool permanent)
        {
            Part part = partSet.GetPart(index);

            if (part != null)
            {
                BacTransformedParts[index] = new CharaPart(GameBase, chara, part, (PartType)index);
                BacPermanentParts[index] = permanent;
            }
        }

        public void ResetBacPartSetSwap(bool revertPermanentSwaps)
        {
            //Always set this to true currently. 
            revertPermanentSwaps = true;

            for (int i = 0; i < BacTransformedParts.Length; i++)
            {
                if (BacPermanentParts[i] == false || revertPermanentSwaps)
                {
                    BacTransformedParts[i] = null;
                    BacPermanentParts[i] = false;
                }
            }
        }
        #endregion

        #region Rendering
        public override void Update()
        {
            for (int i = 0; i < Parts.Length; i++)
            {
                if (BacTransformedParts[i] != null)
                    BacTransformedParts[i].Update();
                else if (TransformedParts[i] != null)
                    TransformedParts[i].Update();
                else if (Parts[i] != null)
                    Parts[i].Update();
            }
        }

        public override void Draw()
        {
            for (int i = 0; i < Parts.Length; i++)
            {
                //Draw Priority:
                //-TransformedPart
                //-Else, Part if no TransformedPart

                if (BacTransformedParts[i] != null)
                    BacTransformedParts[i].Draw(HideFlags);
                else if (TransformedParts[i] != null)
                    TransformedParts[i].Draw(HideFlags);
                else if (Parts[i] != null)
                    Parts[i].Draw(HideFlags);
            }
        }

        public void DrawSimple(bool normalPass)
        {
            for (int i = 0; i < Parts.Length; i++)
            {
                if (BacTransformedParts[i] != null)
                    BacTransformedParts[i].DrawSimple(normalPass, HideFlags);
                else if (TransformedParts[i] != null)
                    TransformedParts[i].DrawSimple(normalPass, HideFlags);
                else if (Parts[i] != null)
                    Parts[i].DrawSimple(normalPass, HideFlags);
            }
        }
        #endregion

        #region BcsColors
        public void ApplyCustomColors()
        {
            foreach (CharaPart part in Parts)
            {
                if (part != null)
                {
                    part.ResetCustomColors();
                    part.LoadCustomColors(BcsColors);
                }
            }
        }

        private void SetBcsColor(int group, int color)
        {
            CustomColorGroup partColor = BcsColors.FirstOrDefault(x => x.Group == group);

            if(partColor == null)
            {
                BcsColors.Add(new CustomColorGroup(group, color));
            }
            else
            {
                partColor.Color = color;
            }
        }

        public void SetBcsColors(PartSet partSet)
        {
            BcsColors.Clear();
            SetBcsColors(partSet.Boots);
            SetBcsColors(partSet.Bust);
            SetBcsColors(partSet.FaceBase);
            SetBcsColors(partSet.FaceEar);
            SetBcsColors(partSet.FaceEye);
            SetBcsColors(partSet.FaceForehead);
            SetBcsColors(partSet.FaceNose);
            SetBcsColors(partSet.Hair);
            SetBcsColors(partSet.Pants);
            SetBcsColors(partSet.Rist);
        }

        private void SetBcsColors(Part part)
        {
            if (part == null) return;

            foreach(ColorSelector colSel in part.ColorSelectors)
            {
                SetBcsColor(colSel.PartColorGroup, colSel.ColorIndex);
            }
        }

        public void SetCacCustomColors(Xv2CoreLib.SAV.CaC cac, int preset)
        {
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.HAIR_, cac.Appearence.I_44);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.EYE_, cac.Appearence.I_46);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.PAINT_A_, cac.Appearence.I_48);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.PAINT_B_, cac.Appearence.I_50);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.PAINT_C_, cac.Appearence.I_52);

            SetBcsColor((int)CustomColorGroup.PartColorMaterials.SKIN_, cac.Appearence.I_36);
            SetBcsColor(1, cac.Appearence.I_38);
            SetBcsColor(2, cac.Appearence.I_40);
            SetBcsColor(3, cac.Appearence.I_42);

            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC00_BUST_, cac.Presets[preset].I_28);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC01_BUST_, cac.Presets[preset].I_30);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC02_BUST_, cac.Presets[preset].I_32);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC03_BUST_, cac.Presets[preset].I_34);

            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC00_PANTS_, cac.Presets[preset].I_36);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC01_PANTS_, cac.Presets[preset].I_38);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC02_PANTS_, cac.Presets[preset].I_40);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC03_PANTS_, cac.Presets[preset].I_42);

            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC00_RIST_, cac.Presets[preset].I_44);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC01_RIST_, cac.Presets[preset].I_46);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC02_RIST_, cac.Presets[preset].I_48);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC03_RIST_, cac.Presets[preset].I_50);

            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC00_BOOTS_, cac.Presets[preset].I_52);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC01_BOOTS_, cac.Presets[preset].I_54);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC02_BOOTS_, cac.Presets[preset].I_56);
            SetBcsColor((int)CustomColorGroup.PartColorMaterials.CC03_BOOTS_, cac.Presets[preset].I_58);
        }

        #endregion

        public bool IsPartSet(PartSet partSet)
        {
            return this.partSet == partSet;
        }

        public bool IsPartSet(Part part)
        {
            return partSet?.Parts?.Contains(part) == true;
        }

        private CharaPart GetActiveEyePartSet()
        {
            if (TransformedParts[(int)PartType.FaceEye] != null) return TransformedParts[(int)PartType.FaceEye];
            if (Parts[(int)PartType.FaceEye] != null) return Parts[(int)PartType.FaceEye];
            return null;
        }

        public float GetLeftEyeScale()
        {
            CharaPart eye = GetActiveEyePartSet();
            return eye != null ? eye.part.F_36 : 1f;
        }

        public float GetRightEyeScale()
        {
            CharaPart eye = GetActiveEyePartSet();
            return eye != null ? eye.part.F_40 : 1f;
        }

        public void GetAABB()
        {
            for(int i = 0; i < Parts.Length; i++)
            {
                if (Parts[i]?.EmdFile != null)
                {
                    foreach(EMD_Model model in Parts[i].EmdFile.Models)
                    {
                        foreach(EMD_Mesh mesh in model.Meshes)
                        {
                            if(mesh.AABB.MinX < chara.AABB[0])
                                chara.AABB[0] = mesh.AABB.MinX;

                            if (mesh.AABB.MinY < chara.AABB[1])
                                chara.AABB[1] = mesh.AABB.MinY;

                            if (mesh.AABB.MinZ < chara.AABB[2])
                                chara.AABB[2] = mesh.AABB.MinZ;

                            if (mesh.AABB.MaxX > chara.AABB[3])
                                chara.AABB[3] = mesh.AABB.MaxX;

                            if (mesh.AABB.MaxY > chara.AABB[4])
                                chara.AABB[4] = mesh.AABB.MaxY;

                            if (mesh.AABB.MaxZ > chara.AABB[5])
                                chara.AABB[5] = mesh.AABB.MaxZ;
                        }
                    }
                }
            }
        }
    }

    public class CharaPart : Entity
    {
        private Actor chara;
        private PartSet parentPartSet;
        public Part part;
        private PhysicsPart physicsPart;
        public PartType partType;
        private PartTypeFlags partTypeFlag;

        //Physics Object stuff:
        private bool IsPhysicsPart;
        private CharaPart Parent;
        private CharaPart[] PhysicsParts;

        //Compiled Objects:
        public Xv2ModelFile Model { get; private set; }
        public List<Xv2ShaderEffect> Materials { get; private set; }
        public Xv2Texture[] Textures { get; private set; }
        public Xv2Texture[] Dyts { get; private set; }
        public Xv2Skeleton Skeleton { get; private set; }
        private int[] SkeletoBoneIndices;

        //Source Files:
        private bool nullPartSet = false;
        public EMD_File EmdFile { get; private set; }
        private EMM_File EmmFile = null;
        private EMB_File EmbFile = null;
        private EMB_File DytFile = null;
        private EAN_File EanFile = null;
        private ESK_File EskFile = null;

        //Dyt Sampler:
        private SamplerInfo DytSampler;

        //Eye Material Index
        private int LeftEyeIndex = -1;
        private int RightEyeIndex = -1;

        //Auto-updating
        private bool IsMaterialsDirty = false;

        public CharaPart(GameBase gameBase, Actor chara, Part part, PartType type) : base(gameBase)
        {
            GameBase = gameBase;
            this.chara = chara;
            this.part = part;
            partType = type;
            partTypeFlag = GetPartTypeAsFlag();
            parentPartSet = chara.CharacterData.BcsFile.File.GetParentPartSet(part);

            DytSampler = new SamplerInfo()
            {
                type = SamplerType.Sampler2D,
                textureSlot = 4,
                samplerSlot = 4,
                name = ShaderManager.GetSamplerName(4),
                state = new SamplerState()
                {
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Wrap,
                    BorderColor = new Microsoft.Xna.Framework.Color(1, 1, 1, 1),
                    Filter = TextureFilter.LinearMipPoint,
                    MaxAnisotropy = 1,
                    MaxMipLevel = 1,
                    MipMapLevelOfDetailBias = 0,
                    Name = ShaderManager.GetSamplerName(4)
                }
            };

            LoadPart();
            LoadChildPhysicsParts();
        }

        public CharaPart(GameBase gameBase, Actor chara, PhysicsPart part, Part parentPart, CharaPart parent, PartType type) : base(gameBase)
        {
            GameBase = gameBase;
            this.chara = chara;
            this.part = parentPart;
            parentPartSet = chara.CharacterData.BcsFile.File.GetParentPartSet(part);
            physicsPart = part;
            partType = type;
            partTypeFlag = GetPartTypeAsFlag();
            Parent = parent;
            IsPhysicsPart = true;

            DytSampler = new SamplerInfo()
            {
                type = SamplerType.Sampler2D,
                textureSlot = 4,
                samplerSlot = 4,
                name = ShaderManager.GetSamplerName(4),
                state = new SamplerState()
                {
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Wrap,
                    BorderColor = new Microsoft.Xna.Framework.Color(1, 1, 1, 1),
                    Filter = TextureFilter.LinearMipPoint,
                    MaxAnisotropy = 1,
                    MaxMipLevel = 1,
                    MipMapLevelOfDetailBias = 0,
                    Name = ShaderManager.GetSamplerName(4)
                }
            };

            LoadPart();
        }

        #region Load
        public void LoadPart()
        {
            LoadSources();

            if (!nullPartSet)
            {
                if (IsPhysicsPart)
                {
                    LoadSkeleton();
                }

                LoadModel(false);
                LoadMaterials();
                LoadTextures();
                LoadDyts();
            }
        }

        public void LoadModel(bool reloadMaterials = true)
        {
            if (EmdFile == null)
            {
                Log.Add($"CharaPart.LoadModel: No emd file has been loaded.", LogType.Error);
                return;
            }

            //Unsubscribe from ModelChanged event if a model already exists on this Part
            if (Model != null)
                Model.ModelChanged -= RefreshMaterialsOnEdit;

            Model = CompiledObjectManager.GetCompiledObject<Xv2ModelFile>(EmdFile, GameBase);

            if (reloadMaterials)
                LoadMaterials();
        }

        public void LoadMaterials()
        {
            if (EmdFile == null)
            {
                Log.Add($"CharaPart.LoadMaterials: No emd file has been loaded. This must be done before materials can be initialized!", LogType.Error);
                return;
            }

            if (EmmFile == null)
            {
                Log.Add($"CharaPart.LoadMaterials: No emm file has been loaded.", LogType.Error);
                return;
            }

            if (Model != null)
            {
                Materials = Model.InitializeMaterials(ShaderType.Chara, EmmFile);

                //If part is an Eye, then set the Eye Indices. This is needed for eye UV scroll to be set to allow eye movement.
                //This code assumes there is just one material per eye
                LeftEyeIndex = RightEyeIndex = -1;

                if (partType == PartType.FaceEye)
                {
                    for (int i = 0; i < Materials.Count; i++)
                    {
                        if (Materials[i].Material.Name.Contains("_L"))
                        {
                            LeftEyeIndex = i;
                        }
                        else if (Materials[i].Material.Name.Contains("_R"))
                        {
                            RightEyeIndex = i;
                        }
                    }
                }

                Model.ModelChanged += RefreshMaterialsOnEdit;
            }
        }

        public void LoadTextures()
        {
            if (EmbFile == null)
            {
                Log.Add($"CharaPart.LoadTextures: No emb file has been loaded.", LogType.Error);
                return;
            }

            Textures = Xv2Texture.LoadTextureArray(EmbFile, GameBase);
        }

        public void LoadDyts()
        {
            if (DytFile == null)
            {
                Log.Add($"CharaPart.LoadDyts: No dyt.emb file has been loaded.", LogType.Error);
                return;
            }

            Dyts = Xv2Texture.LoadTextureArray(DytFile, GameBase);
        }

        public void LoadSkeleton()
        {
            if (EskFile == null)
            {
                Log.Add($"CharaPart.LoadSkeleton: No esk file has been loaded.", LogType.Error);
                return;
            }

            Skeleton = CompiledObjectManager.GetCompiledObject<Xv2Skeleton>(EskFile, GameBase);

            if(Skeleton != null)
            {
                SkeletoBoneIndices = Skeleton.ScdGetBoneIndices(chara.Skeleton);
            }
        }

        public void LoadCustomColors(List<CustomColorGroup> colors)
        {
            foreach(CustomColorGroup color in colors)
            {
                if(color.Color != -1 && color.Group != -1)
                {
                    Xv2CoreLib.BCS.PartColor colorGroup = chara.CharacterData.BcsFile.File.PartColors?.FirstOrDefault(x => x.ID == color.Group);
                    if (colorGroup == null) continue;

                    if (Model != null)
                    {
                        foreach (Xv2Model model in Model.Models)
                        {
                            foreach (Xv2Mesh mesh in model.Meshes)
                            {
                                foreach (Xv2Submesh submesh in mesh.Submeshes)
                                {
                                    if (IsCustomColorMaterial(submesh.Name, colorGroup.Name))
                                    {
                                        submesh.ApplyCustomColor(color.Group, color.Color);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //We have to load colors from ALL the parts, because they draw the colors from all the parts for whatever reason. No idea on the reasoning for this
            //i am now enlightened
            /*
            foreach (var _part in parentPartSet.Parts)
            {
                foreach (var colSel in _part.ColorSelectors)
                {
                    var colorGroup = chara.CharacterData.BcsFile.File.PartColors?.FirstOrDefault(x => x.ID == colSel.PartColorGroup);
                    if (colorGroup == null) continue;

                    if (Model != null)
                    {
                        foreach (var model in Model.Models)
                        {
                            foreach (var mesh in model.Meshes)
                            {
                                foreach (var submesh in mesh.Submeshes)
                                {
                                    if (IsCustomColorMaterial(submesh.Name, colorGroup.Name))
                                    {
                                        submesh.ApplyCustomColor(colSel.PartColorGroup, colSel.ColorIndex);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            */
        }

        public void ResetCustomColors()
        {
            if (Model == null) return;

            foreach (Xv2Model model in Model.Models)
            {
                foreach (Xv2Mesh mesh in model.Meshes)
                {
                    foreach (Xv2Submesh submesh in mesh.Submeshes)
                    {
                        submesh.ResetCustomColor();
                    }
                }
            }
        }

        private void LoadChildPhysicsParts()
        {
            int physicsPartCount = part.PhysicsParts != null ? part.PhysicsParts.Count : 0;

            PhysicsParts = new CharaPart[physicsPartCount];

            for (int i = 0; i < physicsPartCount; i++)
            {
                PhysicsParts[i] = new CharaPart(GameBase, chara, part.PhysicsParts[i], part, this, partType);
            }
        }

        private void RefreshMaterialsOnEdit(object sender, EventArgs e)
        {
            IsMaterialsDirty = true;
        }

        private void RefreshMaterials()
        {
            if (Model != null && EmmFile != null)
            {
                Materials = Model.InitializeMaterials(ShaderType.Chara, EmmFile);
            }
        }
        #endregion

        #region SourceLoad
        private void LoadSources()
        {
            if (!LoadEmd())
            {
                nullPartSet = true;
                return;
            }
            LoadEmb();
            LoadDyt();
            LoadEmm();
            LoadEan();

            if (IsPhysicsPart)
            {
                LoadEsk();
            }

            nullPartSet = false;
        }

        private bool LoadEmd()
        {
            string path;
            string owner;

            if (IsPhysicsPart)
            {
                path = GetFilePath(physicsPart.GetModelPath());
                owner = physicsPart.CharaCode;
            }
            else
            {
                path = GetFilePath(part.GetModelPath(partType));
                owner = part.CharaCode;
            }

            if (path != null)
            {
                //If the file belongs to this character, then we pull the files from the Xv2Character object itself. 
                if (chara.ShortName == owner && Utils.CompareCharaFolder(path, chara.ShortName))
                {
                    var file = GetFileFromOwner(path);

                    if (file != null)
                        EmdFile = (EMD_File)file;
                }
                else
                {
                    //This character doesn't own these files so we just load them directly.
                    EmdFile = (EMD_File)FileManager.Instance.GetParsedFileFromGame(path);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private void LoadEmb()
        {
            string path;
            string owner;

            if (IsPhysicsPart)
            {
                path = GetFilePath(physicsPart.GetEmbPath(), Parent.part.GetEmbPath(partType));
                owner = physicsPart.CharaCode;
            }
            else
            {
                path = GetFilePath(part.GetEmbPath(partType));
                owner = part.CharaCode;
            }

            if (path != null)
            {
                //If the file belongs to this character, then we pull the files from the Xv2Character object itself. 
                if (chara.ShortName == owner && Utils.CompareCharaFolder(path, chara.ShortName))
                {
                    var file = GetFileFromOwner(path);

                    if (file != null)
                        EmbFile = (EMB_File)file;
                }
                else
                {
                    //This character doesn't own these files so we just load them directly.
                    EmbFile = (EMB_File)FileManager.Instance.GetParsedFileFromGame(path);
                }
            }
            else
            {
                Log.Add($"EMB not found: {path}.", LogType.Error);
            }
        }

        private void LoadDyt()
        {
            string path;
            string owner = IsPhysicsPart ? physicsPart.CharaCode : part.CharaCode;

            if (IsPhysicsPart)
            {
                path = GetFilePath(physicsPart.GetDytPath(), Parent.part.GetDytPath(partType));
            }
            else
            {
                path = GetFilePath(part.GetDytPath(partType));
            }

            if (path != null)
            {
                //If the file belongs to this character, then we pull the files from the Xv2Character object itself. 
                if (chara.ShortName == owner && Utils.CompareCharaFolder(path, chara.ShortName))
                {
                    var file = GetFileFromOwner(path);

                    if (file != null)
                        DytFile = (EMB_File)file;
                }
                else
                {
                    //This character doesn't own these files so we just load them directly.
                    DytFile = (EMB_File)FileManager.Instance.GetParsedFileFromGame(path);
                }
            }
            else
            {
                Log.Add($"DYT not found: {path}.", LogType.Error);
            }
        }

        private void LoadEmm()
        {
            string path;
            string owner = IsPhysicsPart ? physicsPart.CharaCode : part.CharaCode;

            if (IsPhysicsPart)
            {
                path = GetFilePath(physicsPart.GetEmmPath(), Parent.part.GetEmmPath(partType));

            }
            else
            {
                path = GetFilePath(part.GetEmmPath(partType));
            }

            if (path != null)
            {
                //If the file belongs to this character, then we pull the files from the Xv2Character object itself. 
                if (chara.ShortName == owner && Utils.CompareCharaFolder(path, chara.ShortName))
                {
                    var file = GetFileFromOwner(path);

                    if (file != null)
                        EmmFile = (EMM_File)file;
                }
                else
                {
                    //This character doesn't own these files so we just load them directly.
                    EmmFile = (EMM_File)FileManager.Instance.GetParsedFileFromGame(path);
                }
            }
            else
            {
                Log.Add($"EMM not found: {path}.", LogType.Error);
            }
        }

        private void LoadEan()
        {
            string path;
            string declaredPath;
            string owner = IsPhysicsPart ? physicsPart.CharaCode : part.CharaCode;

            if (IsPhysicsPart)
            {
                declaredPath = physicsPart.EanPath;
                path = GetFilePath(physicsPart.GetEanPath());

            }
            else
            {
                declaredPath = part.EanPath;
                path = GetFilePath(part.GetEanPath());
            }

            if (path != null)
            {
                //If the file belongs to this character, then we pull the files from the Xv2Character object itself. 
                if (chara.ShortName == owner && Utils.CompareCharaFolder(path, chara.ShortName))
                {
                    var file = GetFileFromOwner(path);

                    if (file != null)
                        EanFile = (EAN_File)file;
                }
                else
                {
                    //This character doesn't own these files so we just load them directly.
                    EanFile = (EAN_File)FileManager.Instance.GetParsedFileFromGame(path);
                }
            }

            if (!string.IsNullOrWhiteSpace(declaredPath) && string.IsNullOrWhiteSpace(path))
            {
                Log.Add($"EanPath was declared but no file could be found: {declaredPath}.", LogType.Error);
            }
        }

        private void LoadEsk()
        {
            string path;

            if (IsPhysicsPart)
            {
                path = GetFilePath(physicsPart.GetEskPath());

            }
            else
            {
                Log.Add($"LoadEsk: Only possible on PhysicsParts!", LogType.Error);
                return;
            }

            if (path != null)
            {
                //If the file belongs to this character, then we pull the files from the Xv2Character object itself. 
                if (chara.ShortName == physicsPart.CharaCode && Utils.CompareCharaFolder(path, chara.ShortName))
                {
                    var file = GetFileFromOwner(path);

                    if (file != null)
                        EskFile = (ESK_File)file;
                }
                else
                {
                    //This character doesn't own these files so we just load them directly.
                    EskFile = (ESK_File)FileManager.Instance.GetParsedFileFromGame(path);
                }
            }
        }

        private string GetFilePath(string path, string pathOnParent = null)
        {
            if (chara.CharacterData.GetPartSetFile(Path.GetFileName(path)) != null)
                return path;

            if (FileManager.Instance.fileIO.FileExists(path))
                return path;

            if (pathOnParent != null)
            {
                if (FileManager.Instance.fileIO.FileExists(pathOnParent))
                    return pathOnParent;
            }

            return null;
        }

        private object GetFileFromOwner(string path)
        {
            var file = chara.CharacterData.GetPartSetFile(Path.GetFileName(path));

            if (file == null)
            {
                Log.Add($"\"{path}\" failed to load.", LogType.Error);
                return null;
            }

            return file;
        }

        public EAN_File GetEan()
        {
            return EanFile;
        }
        #endregion

        #region Rendering
        public override void Update()
        {
            Model?.Update(chara.ActorSlot, chara.Skeleton);

            if (!IsPhysicsPart)
            {
                foreach (CharaPart physicsPart in PhysicsParts)
                {
                    physicsPart.UpdatePhysicsPart();
                }
            }
        }

        public void UpdatePhysicsPart()
        {
            Skeleton?.ScdUpdate(chara.Skeleton, SkeletoBoneIndices);

            Model?.Update(0, Skeleton);
        }

        /// <summary>
        /// Normal part draw function.
        /// </summary>
        public void Draw(PartTypeFlags hideFlags)
        {
            if (IsMaterialsDirty)
            {
                RefreshMaterials();
                IsMaterialsDirty = false;
            }

            //Check if part hiding is enabled for this Part Type
            if (hideFlags.HasFlag(partTypeFlag)) return;

            int texIdx = GetDytIndex(part.Texture);

            if (Dyts != null)
            {
                //Set dyt sampler/texture
                GraphicsDevice.SamplerStates[4] = DytSampler.state;
                GraphicsDevice.VertexSamplerStates[4] = DytSampler.state;

                if (texIdx >= 0 && texIdx <= Dyts.Length - 1)
                {
                    GraphicsDevice.Textures[4] = Dyts[texIdx].Texture;
                    GraphicsDevice.VertexTextures[4] = Dyts[texIdx].Texture;
                }
            }

            //Set eye movement parameters in shaders
            if (partType == PartType.FaceEye)
            {
                if (LeftEyeIndex != -1)
                {
                    Materials[LeftEyeIndex].SetEyeMovement(chara.EyeIrisLeft_UV);
                }

                if (RightEyeIndex != -1)
                {
                    Materials[RightEyeIndex].SetEyeMovement(chara.EyeIrisRight_UV);
                }
            }

            if (Model != null)
            {
                //Draw Model
                Model.Draw(chara.Transform, chara.ActorSlot, Materials, Textures, Dyts, texIdx, chara.Skeleton);
            }

            if (!IsPhysicsPart)
            {
                foreach (var physicsPart in PhysicsParts)
                {
                    physicsPart.DrawPhysicsPart();
                }
            }
        }

        public void DrawPhysicsPart()
        {
            if (IsMaterialsDirty)
            {
                RefreshMaterials();
                IsMaterialsDirty = false;
            }

            int texIdx = GetDytIndex(physicsPart.Texture);

            if (Dyts != null)
            {
                //Set dyt sampler/texture
                GraphicsDevice.SamplerStates[4] = DytSampler.state;
                GraphicsDevice.VertexSamplerStates[4] = DytSampler.state;

                if (texIdx >= 0 && texIdx <= Dyts.Length - 1)
                {
                    GraphicsDevice.Textures[4] = Dyts[texIdx].Texture;
                    GraphicsDevice.VertexTextures[4] = Dyts[texIdx].Texture;
                }
            }

            if (Model != null)
            {
                Model.Draw(chara.Transform, chara.ActorSlot, Materials, Textures, Dyts, texIdx, Skeleton);
            }
        }

        private int GetDytIndex(int partIdx)
        {
            if (chara.ForceDytOverride > -1) return chara.ForceDytOverride;
            return partIdx;
        }

        public void DrawSimple(bool normalPass, PartTypeFlags hideFlags)
        {
            //Check if part hiding is enabled for this Part Type
            if (hideFlags.HasFlag(partTypeFlag)) return;

            if (Model != null)
            {
                //Draw Model
                Model.Draw(chara.Transform, chara.ActorSlot, normalPass ? RenderSystem.NORMAL_FADE_WATERDEPTH_W_M : RenderSystem.ShadowModel_W, chara.Skeleton);
            }

            if (!IsPhysicsPart)
            {
                foreach (CharaPart physicsPart in PhysicsParts)
                {
                    physicsPart.DrawSimplePhysicsPart(normalPass);
                }
            }
        }

        public void DrawSimplePhysicsPart(bool normalPass)
        {
            if (Model != null)
            {
                Model.Draw(chara.Transform, 0, normalPass ? RenderSystem.NORMAL_FADE_WATERDEPTH_W_M : RenderSystem.ShadowModel_W, Skeleton);
            }
        }


        #endregion

        private bool IsCustomColorMaterial(string materialName, string colorMaterialPrefix)
        {
            if (materialName.Length <= colorMaterialPrefix.Length) return false;

            for (int i = 0; i < colorMaterialPrefix.Length; i++)
            {
                if (colorMaterialPrefix[i] != materialName[i]) return false;
            }

            return true;
        }

        public PartTypeFlags GetHideFlags()
        {
            PartTypeFlags flags = 0;

            if (part != null)
            {
                flags |= part.HideFlags;

                if (PhysicsParts != null)
                {
                    for (int i = 0; i < PhysicsParts.Length; i++)
                    {
                        if (PhysicsParts[i] != null)
                        {
                            flags |= PhysicsParts[i].physicsPart.HideFlags;
                        }
                    }
                }
            }

            return flags;
        }

        public PartTypeFlags GetHideMatFlags()
        {
            PartTypeFlags flags = 0;

            if (part != null)
            {
                flags |= part.HideMatFlags;

                if (PhysicsParts != null)
                {
                    for (int i = 0; i < PhysicsParts.Length; i++)
                    {
                        if (PhysicsParts[i] != null)
                        {
                            flags |= PhysicsParts[i].physicsPart.HideMatFlags;
                        }
                    }
                }
            }

            return flags;
        }

        public PartTypeFlags GetPartTypeAsFlag()
        {
            return GetPartTypeAsFlag(partType);
        }

        public static PartTypeFlags GetPartTypeAsFlag(PartType type)
        {
            switch (type)
            {
                case PartType.Boots:
                    return PartTypeFlags.Boots;
                case PartType.Bust:
                    return PartTypeFlags.Bust;
                case PartType.FaceBase:
                    return PartTypeFlags.FaceBase;
                case PartType.FaceEar:
                    return PartTypeFlags.FaceEar;
                case PartType.FaceEye:
                    return PartTypeFlags.FaceEye;
                case PartType.FaceForehead:
                    return PartTypeFlags.FaceForehead;
                case PartType.FaceNose:
                    return PartTypeFlags.FaceNose;
                case PartType.Hair:
                    return PartTypeFlags.Hair;
                case PartType.Pants:
                    return PartTypeFlags.Pants;
                case PartType.Rist:
                    return PartTypeFlags.Rist;
            }

            return 0;
        }
    
    }

    public class CustomColorGroup
    {
        //For CAC reference only. Get the actual material prefix from the BCS file, as it can change!
        public enum PartColorMaterials
        {
            SKIN_ = 0,
            SKIN2 = 1,
            SKIN3 = 2,
            SKIN4 = 3,
            HAIR_ = 4,
            EYE_ = 5,
            CC00_BUST_ = 6,
            CC01_BUST_ = 7,
            CC02_BUST_ = 8,
            CC03_BUST_ = 9,
            CC00_PANTS_ = 10,
            CC01_PANTS_ = 11,
            CC02_PANTS_ = 12,
            CC03_PANTS_ = 13,
            CC00_RIST_ = 14,
            CC01_RIST_ = 15,
            CC02_RIST_ = 16,
            CC03_RIST_ = 17,
            CC00_BOOTS_ = 18,
            CC01_BOOTS_ = 19,
            CC02_BOOTS_ = 20,
            CC03_BOOTS_ = 21,
            PAINT_A_ = 22,
            PAINT_B_ = 23,
            PAINT_C_ = 24
        }

        public int Group { get; set; }
        public int Color { get; set; }

        public CustomColorGroup(int group, int color)
        {
            Group = group;
            Color = color;
        }

        public void Reset()
        {
            Group = -1;
            Color = -1;
        }
    }
}
