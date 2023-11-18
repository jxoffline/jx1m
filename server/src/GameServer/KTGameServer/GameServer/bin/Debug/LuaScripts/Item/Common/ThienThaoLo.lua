-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local ThienThaoLo = Scripts[200003]

local IDX2OneHour = {IDBuff =1998 ,IDItem = 8483}; --Buff ID 1h

local IDX2EightHour = {IDBuff =1997 ,IDItem = 9594}; --Buff Id 8h

local IDBuff = { IDX2OneHour.IDBuff, IDX2EightHour.IDBuff };

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function ThienThaoLo:OnPreCheckCondition(scene, item, player, otherParams)

	-- ************************** --
	--player:AddNotification("Enter -> ThienThaoLo:OnPreCheckCondition")--
	-- ************************** --
	return true
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi để thực thi Logic khi người sử dụng vật phẩm, sau khi đã thỏa mãn hàm kiểm tra điều kiện
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function ThienThaoLo:OnUse(scene, item, player, otherParams)

	-- ************************** --
	local dialog = GUI.CreateItemDialog()
	local nLevel = item:GetItemLevel()
	-- Toác

	-- ************************** --
	 -- xóa vật phẩm

	-- player:AddBuffWithDuration(IDBuff[nLevel],1,ADD_X2EXP[nLevel])  --ckeck lại buff
	if(item:GetItemID()==IDX2EightHour.IDItem)then
		if (player:HasBuff(IDX2OneHour.IDBuff)) then -- ckeck xem có 1 buff trên người hay không nếu có thì xóa
			player:RemoveBuff(IDX2OneHour.IDBuff)
			player:AddNotification("Xóa Tiên Thảo Lộ 1 giờ Thành công!") --thông báo
		end
	end
	if(item:GetItemID()== IDX2OneHour.IDItem)then  
		if (player:HasBuff(IDX2EightHour.IDBuff)) then -- ckeck xem có 1 buff trên người hay không nếu có thì không cho dùng
			player:AddNotification("Ngươi đang dùng Tiên Thảo Lộ Đặc Biệt không thể dùng thêm Tiên Thảo Lộ!") --thông báo
			return;
				end
	end
	Player.RemoveItem(player, item:GetID())
	player:AddBuff(IDBuff[nLevel], 1)
	player:AddNotification("Ngươi Dùng ".. item:GetName() .." Thành công!") --thông báo

	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function ThienThaoLo:OnSelection(scene, item, player, selectionID, otherParams)

	-- ************************** --
	--player:AddNotification("Enter -> ThienThaoLo:OnSelection, selectionID = " .. selectionID)--

	-- ************************** --

	item:FinishUsing(player)


end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function ThienThaoLo:OnItemSelected(scene, item, player, itemID)

	-- ************************** --

	-- ************************** --

end
