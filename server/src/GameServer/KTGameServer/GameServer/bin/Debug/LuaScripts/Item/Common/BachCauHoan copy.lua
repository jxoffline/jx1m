-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local BachCauHoan = Scripts[200004]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function BachCauHoan:OnPreCheckCondition(scene, item, player, otherParams)

	-- ************************** --
	--player:AddNotification("Enter -> BachCauHoan:OnPreCheckCondition")--
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
function BachCauHoan:OnUse(scene, item, player, otherParams)



	local ItemID = item:GetItemID()

	local Itemlevel = item:GetItemLevel()
	
	local ValueInRecore = Player.GetBCH(player,Itemlevel)

	if(ValueInRecore==-1) then

		ValueInRecore = 0

	end


	local MSG = "Số phút Bạch Cầu Hoàn ủy thác hiện tại đang có :"..ValueInRecore.."\nSau khi sử dụng số phút tối đa có thể ủy thác là : "..(480+ValueInRecore).." phút!\nMỗi ngày được ủy thác tối đa 18h"
	
	local dialog = GUI.CreateItemDialog()

	dialog:AddText(MSG)

    dialog:AddSelection(1, "Ta muốn sử dụng bây giờ")
    dialog:AddSelection(2, "Ta suy nghĩ đã")


	dialog:Show(item, player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function BachCauHoan:OnSelection(scene, item, player, selectionID, otherParams)

	local GetValue = Player.GetValueOfDailyRecore(player,item:GetItemID())
	local dialog = GUI.CreateItemDialog()
	if (selectionID == 1) then

		local ItemID = item:GetItemID()
	
		local Itemlevel = item:GetItemLevel()

		local ValueInRecore = Player.GetBCH(player,Itemlevel)

		if(ValueInRecore==-1) then

			ValueInRecore = 0
			
		end
		--local GetBCH= Player.GetBCH(player,Itemlevel)
		--local MSG = "Số phút ủy thác của bạn quá nhiều không thể ủy thác nữa : "..ValueInRecore..""
		if(ValueInRecore >= 10000)then
			dialog:AddText("Số phút ủy thác của bạn quá nhiều không thể ủy thác nữa : "..ValueInRecore.."")
			dialog:Show(item, player)
		else
			local NewValue = ValueInRecore + 480;

			Player.SetBCH(player,NewValue,Itemlevel)
			
			Player.RemoveItem(player,item:GetID())

			GUI.CloseDialog(player)
		end
    else

        if selectionID == 2 then
            GUI.CloseDialog(player)
        end

    end

	
	

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function BachCauHoan:OnItemSelected(scene, item, player, itemID)

	-- ************************** --

	-- ************************** --

end