function AddEventItem(baseModel, eventId) {
	var count = $(".options").length;
	var model = `${baseModel}[${count}]`;
	var itemIdHtml = (eventId != null && eventId != '1') ? `<input name='${model}.ItemId' type = 'hidden' value = '0'/>` : ``;
	var newOption = `
		<div class='options' data-id='${count}'>
			<div class='row'>
				<div class='col-1 d-flex justify-content-center'>
					<div class='row d-flex align-content-center'>
						<div class='border border-primary'>
							<label class='fw-bold' name='ItemNumber'></label>
						</div>
					</div>
				</div>
				<div class='col-11 border border-primary rounded'>
					<div class='row'>
						<div class='col-6 ms-2 me-2 pb-3 border-end'>
							<br/>
							<div class='form-floating mb-3'>
								<input class='form-control' name='${model}.ItemName' id='ItemName' placeholder='Item Name' />
								<label for='ItemName'>Item Name</label>
							</div>
							${itemIdHtml}
							<input name='${model}.EventId' type = 'hidden' value = '${eventId}'/>
							<input name='EventItemImagesViewModels[${count}].EventItemIndex' type = 'hidden' value = '${count}'/>
							<div class='form-floating mb-3'>
								<textarea class='form-control' name='${model}.Description' id='ItemDescription' placeholder='Description' rows='2' style='height:100%'></textarea>
								<label for='ItemDescription'>Description</label>
							</div>
							<div class='row'>
								<label class='col-2 col-form-label' for='eventItem-${count}-amount'>Amount</label>
								<div class='col-3'>
									<input class='form-control' id='eventItem-${count}-amount' name='${model}.Amount' type='number' value='1' />
								</div>
							</div>
							<br/>
							<div>
								<button id='removeNewItemBtn' data-id= '${count}' class='btn btn-secondary' name='removeNewItemBtn' style='width:fit-content'>Remove This Item</button><br/><br/>
							</div>
						</div>
						<div class='col-5'>
							<br/>
							<div class='row'>
								<label>Item Image:</label>
							</div>
							<img id='previewImage[${count}]' class='mt-3 mb-3' src = '' style = 'max-height:400px;max-width:400px' />
							<input type='file' class='mb-3 form-control' name='EventItemImagesViewModels[${count}].EventItemImage' style='margin: 0 auto' accept='.png,.jpg,.jpeg,.gif' onchange='document.getElementById("previewImage[${count}]").src = window.URL.createObjectURL(this.files[0])' 
						</div>
					</div>
				</div>
			</div>
		</div>
		<br/><br>`;
	$(newOption).appendTo(".option-container").hide().fadeIn(1000);
	$(this).val('');
	UpdateItemNumber();
}