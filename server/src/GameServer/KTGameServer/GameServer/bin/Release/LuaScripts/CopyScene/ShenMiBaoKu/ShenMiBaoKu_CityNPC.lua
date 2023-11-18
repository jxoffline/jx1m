-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local ShenMiBaoKu_CityNPC = Scripts[500200]

--[[
<Summary>
<Description>Hàm này được gọi khi người chơi ấn vào NPC</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="npc" Type="GameObject.NPC">NPC tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Returns></Returns>
</Summary>
]]
local ItemShop = {
	[431] = {
		Id = 431, Money = 5000000, Name = "Câu Hồn Ngọc Sơ ", Series = -1, Bin = 1, number = 1
	},
	[432] = {
		Id = 432, Money = 10000000, Name = "Câu Hồn Ngọc Trung", Series = -1, Bin = 1, number = 1
	},
	[433] = {
		Id = 433, Money = 20000000, Name = "Câu Hồn Ngọc Cao", Series = -1, Bin = 1, number = 1
	},
}
function ShenMiBaoKu_CityNPC:OnOpen(scene, npc, player)
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog.AddText(
		"Gần đây lão phu phát hiện được một bảo khố thần bí, nghe nói các nho sinh ở đây đều theo lý học, võ công bất phàm, chẵng lẽ trong bảo khố có bí mật gì chưa được khám phá?")
	dialog:AddText(
		"Phụ bản <color=yellow>Thần Bí Bảo Khố</color> do <color=green>Bang chủ</color> hoặc <color=green>Phó bang chủ</color> mở, trước khi mở yêu cầu các thành viên tập trung xung quanh vị trí của <color=green>Bang chủ</color> hoặc <color=green>Phó bang chủ</color>.")
	dialog:AddText(
		"<color=green>Chú ý: Mỗi tuần chỉ mở 2 lần, thời gian khám phá bảo khố tối đa 2 giờ, bất luận thế nào sau 2 giờ tất cả người chơi sẽ được đưa về thành.</color>")
	-- Nếu có bang và là bang chủ hoặc phó bang chủ
	if player:GetGuildID() > 0 and (player:GetGuildRank() == Global_GuildRank.Master or player:GetGuildRank() == Global_GuildRank.ViceMaster) then
		dialog:AddSelection(2, "Mở Thần Bí Bảo Khố")
		--chỉ có bang chủ mới mở được cửa hàng bang
		if player:GetGuildID() > 0 and (player:GetGuildRank() == Global_GuildRank.Master) then
			dialog:AddSelection(300, "Cửa Hàng Bang cống")
		end

		dialog:AddSelection(3, "Danh sách thành viên xung quanh")
	end
	dialog.AddSelection(9999, "Kết thúc đối thoại")
	dialog.Show(npc, player)
	-- ************************** --
end

--[[
<Summary>
<Description>Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="npc" Type="GameObject.NPC">NPC tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Param Name="selectionID" Type="number">ID chức năng</Param>
<Returns></Returns>
</Summary>
]]
function ShenMiBaoKu_CityNPC:OnSelection(scene, npc, player, selectionID)
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()


	-- ************************** --
	if selectionID == 2 then
		-- Kiểm tra điều kiện
		local ret = EventManager.ShenMiBaoKu_CheckCondition(player)
		-- Nếu không thỏa mãn
		if ret ~= "OK" then
			self:ShowDialog(npc, player, ret)
			return
		end

		-- Bắt đầu Thần Bí Bảo Khố
		EventManager.ShenMiBaoKu_Begin(player)
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 3 then
		-- Nếu không có bang
		if player:GetGuildID() <= 0 then
			self:ShowDialog(npc, player, "Ngươi không có bang hội!")
			return
			-- Nếu không phải Bang chủ hoặc Phó bang chủ
		elseif player:GetGuildRank() ~= Global_GuildRank.Master and player:GetGuildRank() ~= Global_GuildRank.ViceMaster then
			self:ShowDialog(npc, player, "Ngươi không phải Bang chủ hoặc Phó bang chủ!")
			return
		end

		local szMsg = EventManager.ShenMiBaoKu_GetNearByGuildMembersToJoinEventDescription(player)
		self:ShowDialog(npc, player, szMsg)
		return
	end
	if selectionID == 300 then
		local moneyGuildOld = Player.CheckGuildStoreMoney(player);
		local szMsg = "Tiền bang cống của bang người còn " .. moneyGuildOld .. " Hãy chi tiêu sao cho hợp lý.";
		dialog:AddText(szMsg)
		for key, value in pairs(ItemShop) do
			dialog:AddSelection(key, value.Name .. " Giá " .. value.Money .. "")
		end
		dialog.AddSelection(9999, "Kết thúc đối thoại")
		dialog:Show(npc, player)
		return
	end
	if selectionID >= 431 and selectionID <= 433 then
		local moneyGuildOld = Player.CheckGuildStoreMoney(player);
		local moneyGuilNew = 0;
		local selectionItem = selectionID + 100000;
		moneyGuilNew = moneyGuildOld - ItemShop[selectionID].Money;
		local NameItem = ItemShop[selectionID].Name;
		if (moneyGuilNew < 0) then
			dialog:AddText("Tiền bang cống của bang người không đủ ")
			dialog.AddSelection(9999, "Kết thúc đối thoại")
			dialog:Show(npc, player)
			return
		end
		dialog:AddText("Tiền bang cống của bang người còn " ..
		moneyGuilNew .. " Khi mua vật phẩm " .. NameItem .. "!")
		dialog.AddSelection(selectionItem, "đồng ý")
		dialog.AddSelection(9999, "Kết thúc đối thoại")
		dialog:Show(npc, player)
	end
	if selectionID >= 431 + 100000 and selectionID <= 433 + 100000 then
		local selectionItem = selectionID - 100000;
		GUI.CloseDialog(player)
		ShenMiBaoKu_CityNPC:Shop(npc, player, ItemShop[selectionItem].Id, ItemShop[selectionItem].Money,
			ItemShop[selectionItem].Series, ItemShop[selectionItem].Bin, ItemShop[selectionItem].Name,
			ItemShop[selectionItem].number)
	end


	if selectionID == 9999 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
end

--[[
<Summary>
<Description>Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi NPC thông qua NPC Dialog</Description>
<Param Name="scene" Type="GameObject.Scene">Bản đồ hiện tại</Param>
<Param Name="npc" Type="GameObject.NPC">NPC tương ứng</Param>
<Param Name="player" Type="GameObject.Player">Người chơi tương ứng</Param>
<Param Name="selectedItemInfo" Type="GameObject.Item">Vật phẩm được chọn</Param>
<Returns></Returns>
</Summary>
]]
function ShenMiBaoKu_CityNPC:OnItemSelected(scene, npc, player, selectedItemInfo)
	-- ************************** --

	-- ************************** --
end

function ShenMiBaoKu_CityNPC:Shop(npc, player, IDItem, Money, Series, Bin, Name, number)
	-- ************************** --.
	local dialog = GUI.CreateItemDialog()
	local moneyGuildOld = Player.CheckGuildStoreMoney(player);
	local moneyGuilNew = 0;
	--ckeck xem bang có đủ tiền không nếu tiền vật phẩm nhiều hơn tiền bang thì cút
	if (Money > moneyGuildOld) then
		player:AddNotification(
			"Tiền Bang của ngươi không đủ không thể mua được đồ này")
		GUI.CloseDialog(player)
		return
	end
	--ckeck xem có phải bang chủ hay
	if player:GetGuildID() > 0 and (player:GetGuildRank() == Global_GuildRank.Master) then
		moneyGuilNew = moneyGuildOld - Money;

		if Player.SetGuildMoneyStore(player, moneyGuilNew) == true then
			if Player.AddItemLua(player, IDItem, number, Series, Bin) == true then
				Player.GuildChat(player, "Bang chủ đã mua vật phẩm " .. Name .. " với giá là: " ..
				Money .. " Bang Cống")
				return;
			else
				dialog:AddText("Lỗi không có vật phẩm")
				return;
			end
		end
	else
		dialog:AddText("Chỉ có bang chủ mới mua được")
		dialog:Show(npc, player)
		return;
	end
	-- ************************** --
end

function ShenMiBaoKu_CityNPC:ShowDialog(npc, player, text)
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:Show(npc, player)
	-- ************************** --
end
