-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000013' bên dưới thành ID tương ứng
local BaiHuTangChuanSongRen_ChengShi = Scripts[400012]

-- ************************** --
local ToSceneInfo = {
	[1] = {
		SceneID = 43,			-- Đại điện Bạch Hổ Đường (sơ)
		PosX = 5170,
		PosY = 2570,
	},
	[2] = {
		SceneID = 51,			-- Đại điện Bạch Hổ Đường (cao)
		PosX = 5170,
		PosY = 2570,
	},
}
-- ************************** --

--****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function BaiHuTangChuanSongRen_ChengShi:OnOpen(scene, npc, player, otherParams)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText("Gần đây, trong <color=yellow>Bạch Hổ Đường</color> xuất hiện đầy bọn trộm cắp, bọn ta không đủ sức đối phó với chúng. Ngươi có thể giúp một tay không? Ta có thể đưa ngươi đến <color=yellow>Đại Điện Bạch Hổ Đường</color>, sự việc cụ thể ngươi có thể tìm hiểu ở chỗ <color=green>Môn đồ Bạch Hổ Đường</color>.")
	dialog:AddSelection(1,"[Quy tắc hoạt động]")
	dialog:AddSelection(2,"<color=yellow>Ta muốn vào Bạch Hổ Đường (sơ)</color>")
	dialog:AddSelection(3,"<color=yellow>Ta muốn vào Bạch Hổ Đường (cao)</color>")
	--dialog:AddSelection(4,"Mua trang bị danh vọng Bạch Hổ Đường")
	dialog:AddSelection(100,"Kết Thúc đối thoại")
	dialog:Show(npc, player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function BaiHuTangChuanSongRen_ChengShi:OnSelection(scene, npc, player, selectionID, otherParams)

	-- ************************** --
	if selectionID == 100 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 1 then
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText("Thời gian báo danh 30 phút, thời gian hoạt động 30 phút. Sau khi hoạt động bắt đầu, trong Bạch Hổ Đường sẽ xuất hiện rất nhiều <color=red>Sấm Đường Tặc</color>, đánh bại chúng sẽ nhặt được vật phẩm và kinh nghệm. Sau một thời gian nhất định sẽ xuất hiện <color=red>Thủ Lĩnh Sấm Dường Tặc</color>, đánh bại <color=red>Thủ Linh Sấm Đường Tặc</color> sẽ xuất hiện lối vào tâng 2. Bạch Hổ Đường có 3 tâng, nếu bạn đánh bại thủ lĩnh ở cả 3 tầng thì sẽ mở được lối ra.<br><color=yellow><u>Lưu ý:</u></color> Khi vào Bạch Hổ Đường sẽ tự động bật chế độ chiến đấu bang hội, nên tốt nhất hãy tham gia hoạt động này cùng với hảo hữu hoặc bang hội (Mỗi ngày chỉ được tham gia tối đa <color=orange>1 lần</color>).")	
		dialog:AddSelection(100, "Kết thúc đối thoại")
		dialog:Show(npc, player)
		return
	end
	-- ************************** --
	if selectionID == 2 then
		player:ChangeScene(ToSceneInfo[1].SceneID, ToSceneInfo[1].PosX, ToSceneInfo[1].PosY)
		return
	end
	-- ************************** --
	if selectionID == 3 then
		player:ChangeScene(ToSceneInfo[2].SceneID, ToSceneInfo[2].PosX, ToSceneInfo[2].PosY)
		return
	end
	-- ************************** --
	if selectionID == 4 then
		if player:GetLevel() >= 10 then
			Player.OpenShop(npc, player, 81)
			GUI.CloseDialog(player)
		else
			local dialog = GUI.CreateNPCDialog()
			dialog:AddText("Chưa đủ cấp!")
			dialog:Show(npc, player)
		end
		return
	end
	-- ************************** --
end
	

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function BaiHuTangChuanSongRen_ChengShi:OnItemSelected(scene, npc, player, itemID, otherParams)

	-- ************************** --
	
	-- ************************** --

end
