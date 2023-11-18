-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local LongNgu = Scripts[0000028]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
-- ****************************************************** --
function LongNgu:OnOpen(scene, npc, player)

	-- ************************** --
        local dialog = GUI.CreateNPCDialog()
        dialog.AddText("Loạn thế phong vân, binh khởi tứ phương, ta có vài nhiệm vụ phải nhờ ngươi giúp!")
        dialog.AddSelection(1,"Ta đến xem giới thiệu về nhiệm vụ.")
        dialog.AddSelection(9999,"Sau này hãy nói.")
        dialog.Show(npc,player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function LongNgu:OnSelection(scene, npc, player, selectionID)

	-- ************************** --
	if selectionID == 9999 then
       	GUI.CloseDialog(player)
	-- ************************** --
    end
    if selectionID == 1 then
        local dialog = GUI.CreateNPCDialog()
        dialog.AddText("Hệ thống nhiệm vụ mới chia thành <color=#FF0000>sơ nhập, chính tuyến và phụ tuyến. Nhiệm vụ sơ nhập</color>: tất cả người chơi đều có thể tiếp nhận, mục đích để giới thiệu những đặc sắc và thao tác trong Võ Lâm Truyền Kỳ. Có thế bỏ <color=#FF0000>nhiệm vụ sơ nhập</color>, nhưng sau này sẽ không thể làm được. <color=#FF0000>Nhiệm vụ chủ tuyến</color>: từ cấo <color=#FF0000>20</color> đến <color=#FF0000>cấp 60</color>, mỗi <color=#FF0000>10 cấp</color> có một nhiệm vụ, gồm 3 phe: <color=#FF0000>Chính phái, Trung lập, Tà phái</color>. Phải gia nhập môn mới có thể tiếp nhận nhiệm vụ.Sau khi hoàn thành sẽ được Trang bị Hoàng Kim và điểm kinh nghiệm. <color=#FF0000>Nhiệm vụ phụ tuyến</color> phải tiếp nhận nhiệm vụ tương ứng với đẳng cấp mới có thể hoàn thành nếu bạn muốn tiếp nhận nhiệm vụ phụ tuyến từ cấp 30 đến 39, bạn phải tiếp nhận nhiệm vụ chủ tuyến Tà phái cấp 30 trước. Khi đang làm nhiệm vụ sơ nhập không thể tiếp nhận nhiệm vụ phụ tuyến. Giải thưởng của các nhiệm vụ rất phong phú, hy vọng bạn có thể đắm mình và tận thưởng.<color=#FF0000> Nếu người chơi mơi bước vào thế giới của trò chơi chúng ta sẽ cho một người bạn động hành cùng bạn phiêu bạt giang hồ.Chỉ cần nhấn 'ta muốn làm nhiệm vụ cùng bạn đồng hành' là co thể chọn bạn đồng hành.</color>")
       	dialog.AddSelection(101,"Kết thúc đối thoại")
		dialog:Show(npc, player)
	elseif selectionID == 101 then
		GUI.CloseDialog(player)
		return
	end
    -- ************************** --
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		selectedItemInfo: SelectItem - Vật phẩm được chọn
--		otherParams: Key-Value {number, string} - Danh sách các tham biến khác
-- ****************************************************** --
function LongNgu:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)

	-- ************************** --
	
	-- ************************** --

end