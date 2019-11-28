using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace cultist_automation
{
    [BepInEx.BepInPlugin("net.wywzxxz.CultistSimulator.CultistAutomation", "CultistAutomation", "0.0.1")]
    public class CultistAutomation : BepInEx.BaseUnityPlugin
    {        
        void Start()
        {
            this.Logger.LogInfo("Cultist Automation initialized.");
        }
        void Update()
        {
            //for DEBUG
            if (!TabletopManager.IsInMansus() && Input.GetKeyDown(KeyCode.F10)) {
                var situation = this.GetOpenSituation();
                var data = situation.GetSaveData();
                foreach (System.Collections.DictionaryEntry r in data)
                    this.Logger.LogInfo("F10:"+r.Key+" "+r.Value);
            }
            if (!TabletopManager.IsInMansus() && Input.GetKeyDown(KeyCode.F9))  //display solution info
            {
                var situation = this.GetOpenSituation();
                var token = situation.situationToken as SituationToken;
                this.Logger.LogInfo("///////////////////////");
                this.Logger.LogInfo("SituList:");
                var sc = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();                
                foreach (var s in sc)
                    this.Logger.LogInfo("     "+s.GetTokenId()+" "+ situDic.ContainsKey(s.situationToken.EntityId));

                this.Logger.LogInfo("Situ:" + situation.GetTokenId());                
                if (situation.SituationClock!=null)
                {
                    this.Logger.LogInfo("    State:" + situation.SituationClock.State);
                    this.Logger.LogInfo("    TimeRemaining:" + situation.SituationClock.TimeRemaining);
                    try
                    {
                        this.Logger.LogInfo("    Warmup:" + situation.SituationClock.Warmup);
                    }catch (Exception e) { }
                }
                this.Logger.LogInfo("EntityId:" + situation.situationToken.EntityId);
                this.Logger.LogInfo("Token:"+ token.name);
                this.Logger.LogInfo("    enabled:" + token.enabled);
                this.Logger.LogInfo("    Defunct:" + token.Defunct);
                this.Logger.LogInfo("    IsBeingAnimated:" + token.IsBeingAnimated);
                this.Logger.LogInfo("    IsInAir:" + token.IsInAir);
                this.Logger.LogInfo("    IsTransient:" + token.IsTransient);
                this.Logger.LogInfo("    NoPush:" + token.NoPush);

                var slots = situation.situationWindow.GetStartingSlots();
                if (situation.SituationClock.State == SituationState.Ongoing)
                    slots = situation.situationWindow.GetOngoingSlots();
                this.Logger.LogInfo("Slots:");
                for (var id = 0;id<slots.Count;++id)
                {
                    var slot = slots[id];                    
                    this.Logger.LogInfo("    Slot"+id+":");
                    this.Logger.LogInfo("        enabled:" + slot.enabled);
                    this.Logger.LogInfo("        isActiveAndEnabled:" + slot.isActiveAndEnabled);                    
                    var ele = slot.GetElementStackInSlot();                    
                    if (ele == null)
                        this.Logger.LogInfo("        No Card");
                    if (ele==null) continue;                    
                    this.Logger.LogInfo("        EntityId:" + ele.EntityId);
                    this.Logger.LogInfo("        Decays:" + ele.Decays);
                    this.Logger.LogInfo("        IsBeingAnimated:" + slot.IsBeingAnimated);
                    this.Logger.LogInfo("        LifetimeRemaining:" + ele.LifetimeRemaining);
                    this.Logger.LogInfo("        Quantity:" + ele.Quantity);                    
                }
                var sid = situation.situationToken.EntityId;
                if (situDic.ContainsKey(sid))
                {
                    var json = situDic[sid];
                    this.Logger.LogInfo("recordThisTurn:" + json.recordThisTurn);
                    this.Logger.LogInfo("turn:" + json.turn);
                    this.Logger.LogInfo("SkipTurn:" + json.SkipTurn);                    
                    this.Logger.LogInfo("curRecipe:" + showRecipe(json.curRecipe));
                    this.Logger.LogInfo("applyedRecipe:" + showRecipe(json.applyedRecipe));                    
                }
            }
            //自动化的控制
            if (!TabletopManager.IsInMansus() && Input.GetKeyDown(KeyCode.F1))
            {
                var situation = this.GetOpenSituation();
                var token = situation.situationToken as SituationToken;
                var id = situation.situationToken.EntityId;
                if (situDic.ContainsKey(id))
                {
                    situDic[id].recordThisTurn = true;                    
                }
                this.Notify("automation", "Record this turn until it ends then try to reproduce.");
            }
            if (!TabletopManager.IsInMansus() && Input.GetKeyDown(KeyCode.F2))
            {
                var situation = this.GetOpenSituation();
                var token = situation.situationToken as SituationToken;
                var id = situation.situationToken.EntityId;
                if (situDic.ContainsKey(id))
                {
                    situDic[id].applyedRecipe = null;
                }
                this.Notify("automation", "Canceled automation for this solution.");
            }
            if (!TabletopManager.IsInMansus() && Input.GetKeyDown(KeyCode.F3))
            {
                var situation = this.GetOpenSituation();
                var token = situation.situationToken as SituationToken;
                var id = situation.situationToken.EntityId;
                if (situDic.ContainsKey(id))
                {
                    situDic[id].SkipTurn = situDic[id].turn + 1;
                }
                this.Notify("automation", "Skip next turn.");
            }
            //监控Situation变化
            watch();
        }
        ///////////////////////////////////////////////////////////////
        class SituationConfig
        {            
            public SituationState status= SituationState.Unstarted;
            public List<List<string>> curRecipe=null;
            public List<List<string>> applyedRecipe = null;
            public bool recordThisTurn=false;
            public float TimeRemaining =-1;
            public long turn = 0;
            public long SkipTurn = -1;
        }
        Dictionary<string, SituationConfig> situDic = new Dictionary<string, SituationConfig>();
        string showRecipe(List<List<string>> recipe)
        {
            if (recipe == null) return "null";
            string res = "[";
            for (var j = 0; j <recipe.Count; ++j)
            {
                res += "<";
                for (var i = 0; i < recipe[j].Count; ++i)
                {
                    if (i != 0) 
                        res += ",";
                    res += recipe[j][i];
                }
                res += ">";
            }
            res += "]";
            return res;
        }
        void watch()
        {

            var sc = Registry.Retrieve<SituationsCatalogue>().GetRegisteredSituations();                
            foreach (var situation in sc)
            {
                try
                {
                    var id = situation.situationToken.EntityId;                    
                    if (!situDic.ContainsKey(id))
                    {
                        situDic[id] = new SituationConfig();
                        if (situation.SituationClock.State == SituationState.Unstarted)
                        {
                            situDic[id].curRecipe = new List<List<string>>();
                            situDic[id].curRecipe.Add(new List<string>());
                        }
                    }
                    var json = situDic[id];
                    
                    if (situation.SituationClock == null) continue;                    
                    switch (situation.SituationClock.State)
                    {
                        case SituationState.Unstarted:                            
                            if (json.status != situation.SituationClock.State)
                            {
                                //this.Logger.LogInfo(id + " new turn");
                                if (situDic[id].curRecipe == null)
                                    situDic[id].curRecipe = new List<List<string>>();
                                json.curRecipe.Clear();
                                json.curRecipe.Add(new List<string>());
                                
                                //this.Logger.LogInfo(id+" Applying recipe " + showRecipe(json.applyedRecipe));
                            }
                            //自动化
                            if (json.applyedRecipe != null && json.turn>json.SkipTurn)
                            {                                
                                var ready=applyRecipeStage(situation, json.applyedRecipe[json.curRecipe.Count - 1]);
                                if (ready)
                                    situation.AttemptActivateRecipe();
                            }                            
                            //记录牌面
                            if(situDic[id].curRecipe!=null)
                                watch_recordCards(situation, json.curRecipe[json.curRecipe.Count - 1]);
                            //this.Logger.LogInfo(id + " recipe > " + showRecipe(json.curRecipe));
                            json.status = situation.SituationClock.State;
                            break;
                        case SituationState.Ongoing:
                            if (json.status != situation.SituationClock.State)//状态变化导致的阶段变化
                            {                                
                                if (situDic[id].curRecipe != null)
                                    json.curRecipe.Add(new List<string>());
                                //监控重要事件
                                if (id == "despair" && situation.GetSaveData()["recipeId"] == "despairdeath")
                                {
                                    this.Notify("automation", "Despair!I'm DYING!!!");
                                    var tabletopManager = (TabletopManager)Registry.Retrieve<ITabletopManager>();
                                    tabletopManager.SetPausedState(true);
                                    SoundManager.PlaySfx("CardDragFail");
                                }
                            }
                            else
                            if (situation.SituationClock.TimeRemaining > json.TimeRemaining)//进入下一个阶段
                            {                                
                                if (situDic[id].curRecipe != null)
                                    json.curRecipe.Add(new List<string>());

                            }
                            json.TimeRemaining = situation.SituationClock.TimeRemaining;
                            //记录牌面     
                            if (situDic[id].curRecipe != null)
                                watch_recordCards(situation, json.curRecipe[json.curRecipe.Count - 1]);
                            //自动化
                            if (json.applyedRecipe != null && json.turn > json.SkipTurn)
                            {
                                applyRecipeStage(situation, json.applyedRecipe[json.curRecipe.Count - 1]);
                            }
                            //重要事件提醒despairdeath
                            json.status = situation.SituationClock.State;
                            break;
                        case SituationState.Complete:
                            if (json.status != situation.SituationClock.State)
                            {                                
                                json.turn += 1;
                                //重要事件提醒
                                if (id == "illhealth")
                                {
                                    this.Notify("automation", "Illness!I'm DYING!!!");
                                    var tabletopManager = (TabletopManager)Registry.Retrieve<ITabletopManager>();
                                    tabletopManager.SetPausedState(true);
                                    SoundManager.PlaySfx("CardDragFail");
                                }
                            }
                            //记录
                            if (json.recordThisTurn && situDic[id].curRecipe != null)
                            {
                                json.recordThisTurn = false;
                                json.applyedRecipe = new List<List<string>>();
                                foreach (var i in json.curRecipe)
                                {
                                    var t = new List<string>();
                                    foreach (var j in i) t.Add(j);
                                    json.applyedRecipe.Add(t);
                                }
                            }
                            json.status = situation.SituationClock.State;
                            //自动收取卡牌
                            if (json.recordThisTurn || json.applyedRecipe != null)
                            {
                                if (id == "dream")
                                {
                                    if(situation.GetOutputStacks().Count()>0)
                                        situation.situationWindow.DumpAllResultingCardsToDesktop();
                                }
                                else
                                    situation.situationWindow.DumpAllResultingCardsToDesktop();
                                
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    this.Logger.LogError("watch error" + e.Message);
                }
        }
        }
        void watch_recordCards(SituationController situation, List<string> recipe)
        {
            recipe.Clear();
            var slots = situation.situationWindow.GetStartingSlots();
            if (situation.SituationClock.State == SituationState.Ongoing)
                slots = situation.situationWindow.GetOngoingSlots();
            for (var id = 0; id < slots.Count; ++id)
            {                
                var slot = slots[id];                
                var ele = slots[id].GetElementStackInSlot();
                if (slot.GetElementStackInSlot() == null) continue;
                if (ele == null) continue;
                if (slot.Defunct || slot.IsGreedy || slot.IsBeingAnimated) {
                    recipe.Add("<empty>");
                    continue;
                }
                recipe.Add(ele.EntityId);
            }
        }
        bool applyRecipeStage(SituationController situation, List<string> ricepeStage)
        {
            var slots = situation.situationWindow.GetStartingSlots();
            if (situation.SituationClock.State == SituationState.Ongoing)
                slots = situation.situationWindow.GetOngoingSlots();
            else
               if (situation.SituationClock.State != SituationState.Unstarted)
                return false;            
            for (var i = 0; i < ricepeStage.Count; ++i)
                if ( (i>=slots.Count || slots[i].GetElementStackInSlot()==null ) && this.GetStackForElement(ricepeStage[i]) == null)
                {
                    //this.Logger.LogInfo("Failed fatching "+ ricepeStage[i]);
                    return false;
                }            
            for (var i = 0; i < ricepeStage.Count; ++i) 
            {
                if (slots[i].GetElementStackInSlot() != null) continue;
                if (ricepeStage[i] == "<empty>") continue;
                var stack = this.GetStackForElement(ricepeStage[i]);                
                this.PopulateSlot(slots[i], stack);
                slots = situation.situationWindow.GetStartingSlots();
                if (situation.SituationClock.State == SituationState.Ongoing)
                    slots = situation.situationWindow.GetOngoingSlots();
            }
            return true;
        }
        ///////////////////////////////////////////////////////////////
        void PopulateSlot(RecipeSlot slot, ElementStackToken stack)
        {                        
            if (stack.Quantity != 1)
                stack = stack.SplitAllButNCardsToNewStack(stack.Quantity-1, new Context(Context.ActionSource.PlayerDrag)) as ElementStackToken;
            stack.lastTablePos = new Vector2?(stack.RectTransform.anchoredPosition);
            slot.AcceptStack(stack, new Context(Context.ActionSource.PlayerDrag));
        }
        ElementStackToken GetStackForElement(string elementId)
        {
            var tokens = this.TabletopTokenContainer.GetTokens();
            var elementStacks =
                from token in tokens
                let stack = token as ElementStackToken
                where stack != null && stack.EntityId == elementId
                select stack;
            elementStacks.Take(1);
            return elementStacks.FirstOrDefault();
        }
        SituationController GetOpenSituation()
        {
            try
            {
                var situation = Registry.Retrieve<SituationsCatalogue>().GetOpenSituation();
                var token = situation.situationToken as SituationToken;
                if (token.Defunct || token.IsBeingAnimated) return null;
                return situation;
            }
            catch (Exception e){}
            return null;
        }
        ///////////////////////////////////////////////////////////////
        private TabletopTokenContainer TabletopTokenContainer
        {
            get
            {
                {
                    var tabletopManager = (TabletopManager)Registry.Retrieve<ITabletopManager>();
                    if (tabletopManager == null)
                    {
                        this.Logger.LogError("Could not fetch TabletopManager");
                        return null;
                    }

                    return tabletopManager._tabletop;
                }
            }
        }
        void Notify(string title, string text)
        {
            Registry.Retrieve<INotifier>().ShowNotificationWindow(title, text);
        }
    }

    class RecipeConfig
    {
        public string Situation;
        public string[] RecipeElements;
        public long waitUntil = 0;
    }
}