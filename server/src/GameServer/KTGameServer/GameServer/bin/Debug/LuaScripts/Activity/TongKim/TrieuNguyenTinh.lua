-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000018' bên dưới thành ID tương ứng
local TrieuNghuyenTinh = Scripts[005018]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function TrieuNghuyenTinh:OnOpen(scene, npc, player)

    -- ************************** --
    local dialog = GUI.CreateNPCDialog()
    if player:GetFactionID()== 0 then
        dialog:AddText("".. npc:GetName() .."Bạn chưa gia nhập môn phái,gia nhập môn phái rồi quay lại!")
        dialog:AddSelection(5, "Kết thúc đối thoại")
        dialog:Show(npc, player)
    else
        dialog:AddText("".. npc:GetName() ..": Gần đây đại Mông Cổ ý Bắc phạt, thu phục Trung Nguyên, nay hai quân đang đối đầu tại biên ải, Mộ Binh Lệnh đã truyền khắp nơi, ta đang định chiêu mộ nhân sĩ võ lâm, trợ giúp quân Tống khôi phục sơn hà<br>Cấp độ của bạn phù hợp với.."..Player.GetBattleNameCanJoin(player))
		if scene:GetID() ~= 76 then
			dialog:AddSelection(1, "Báo danh <color=#FFFF00>Chiến trường Sơ Cấp (Tống) </color>")
			dialog:AddSelection(2, "Báo danh <color=#FFFF00>Chiến trường Cao Cấp (Tống)</color>")
			dialog:AddSelection(3, "Báo danh <color=#FFFF00>Chiến trường Siêu Cấp (Tống)</color>")
		else
			dialog:AddSelection(4, "Báo danh <color=#FFFF00>Chiến trường Liên Máy Chủ Cao Cấp (Tống)</color>")
			dialog:AddSelection(5, "Báo danh <color=#FFFF00>Chiến trường Liên Máy Chủ Siêu Cấp (Tống)</color>")
		end
		dialog:AddSelection(6, "Ta sức hèn tài mọn , đợi khi tinh thông võ nghệ sẽ đến giúp ngươi.")
        dialog:Show(npc, player)
    end
    -- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function TrieuNghuyenTinh:OnSelection(scene, npc, player, selectionID)
 
 
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
    if selectionID == 1 then
        Player.JoinSongJinBattle(npc, player, "TONG", 1)
    elseif selectionID == 2 then
        Player.JoinSongJinBattle(npc, player, "TONG", 2)
    elseif selectionID == 3 then
        Player.JoinSongJinBattle(npc, player, "TONG", 3)
    elseif selectionID == 4 then
        Player.JoinSongJinBattle(npc, player, "TONG", 4)
    elseif selectionID == 5 then
        Player.JoinSongJinBattle(npc, player, "TONG", 5)
	end
	if selectionID == 6 then
		GUI.CloseDialog(player)	
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
function TrieuNghuyenTinh:OnItemSelected(scene, npc, player, itemID)

    -- ************************** --

    -- ************************** --

end
