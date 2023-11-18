-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng


local LenhBaiThiDauMonPhai = Scripts[200018]
local ADD_SHENGWANG = {30, 60, 100}
local USED_LIMIT	= 5 ;
-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function LenhBaiThiDauMonPhai:OnPreCheckCondition(scene, item, player, otherParams)

	-- ************************** --
	--player:AddNotification("Enter -> LenhBaiThiDauMonPhai:OnPreCheckCondition")--
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
function LenhBaiThiDauMonPhai:OnUse(scene, item, player, otherParams)

	local nLevel = item:GetItemLevel()

	if (nLevel < 1 or nLevel > 3) then
		player:AddNotification("Vật phẩm không hợp lệ!")
		return;

	end

	if player:GetFactionID()==0 then
		player:AddNotification("Chưa nhập môn phái!")
		return;
	end
	-- số ngày n ăn được bao nhiêu 
	local GetValue = Player.GetValueOfDailyRecore(player,item:GetItemID())
	if (GetValue > USED_LIMIT) then -- một ngày có thể ăn được bao nhiêu lệnh bài
		player:AddNotification("Mỗi ngày chỉ có thể sử dụng 5 vật phẩm này")
		return;
	end

	if(GetValue==-1) then
		GetValue = 1;
	end
	
	Player.SetValueOfDailyRecore(player,item:GetItemID(),GetValue+1)

	local szFactionName = player:GetFactionName();

	-- lấy phái
	local CAMP = 300 + player:GetFactionID();
	--loại lệnh bài
	local POINT = ADD_SHENGWANG[nLevel];
	--AddRetupeValue : cậu lệnh ăn lệnh bài
	Player.AddRetupeValue(player,CAMP,POINT)
	Player.RemoveItem(player,item:GetID())
	player:AddNotification("Nay ngươi đã dùng "..GetValue.." "..item:GetName().."");
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function LenhBaiThiDauMonPhai:OnSelection(scene, item, player, selectionID, otherParams)

	
	

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function LenhBaiThiDauMonPhai:OnItemSelected(scene, item, player, itemID)

	-- ************************** --

	-- ************************** --

end